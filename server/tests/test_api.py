import json
import os
import tempfile
import time
import asyncio
import unittest
from unittest.mock import patch

import httpx

from server.security import hmac_b64


def _sign_headers(*, secret: str, player_id: str, nonce: str, ts: int, raw_body: str) -> dict:
    payload = f"{player_id}|{nonce}|{ts}|{raw_body or ''}"
    sig = hmac_b64(secret, payload)
    return {
        "X-Nonce": nonce,
        "X-Timestamp": str(ts),
        "X-Signature": sig,
    }


class TestApi(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls._tmp = tempfile.TemporaryDirectory(prefix="kbbq_idle_server_test_")
        cls.db_path = os.path.join(cls._tmp.name, "kbbq_test.db")
        os.environ["KBBQ_DB_PATH"] = cls.db_path
        os.environ["KBBQ_HMAC_SECRET"] = "unit-test-secret"
        os.environ["KBBQ_TOKEN_SALT"] = "unit-test-salt"
        os.environ["KBBQ_MAX_CLOCK_SKEW_SECONDS"] = "9999"
        os.environ["KBBQ_OPS_TOKEN"] = "unit-ops-token"

        from server.app import app

        cls.app = app

    @classmethod
    def tearDownClass(cls):
        try:
            cls._tmp.cleanup()
        except Exception:
            pass

    def test_root_and_health(self):
        r = self._request("GET", "/")
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

        r = self._request("GET", "/health")
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

    def test_auth_then_signed_leaderboard_and_replay_protection(self):
        # 1) Guest auth
        r = self._request("POST", "/auth/guest", json={"deviceId": "device-test-001"})
        self.assertEqual(r.status_code, 200)
        data = r.json()
        player_id = data["playerId"]
        token = data["token"]

        # 2) Signed request should work
        secret = os.environ["KBBQ_HMAC_SECRET"]
        ts = int(time.time())
        nonce = "nonce-top-1"
        headers = {
            "Authorization": f"Bearer {token}",
            **_sign_headers(secret=secret, player_id=player_id, nonce=nonce, ts=ts, raw_body=""),
        }
        r = self._request("GET", "/leaderboard/top?region=KR&limit=5", headers=headers)
        self.assertEqual(r.status_code, 200)
        self.assertIn("entries", r.json())

        # 3) Reusing the same nonce should fail (replay protection)
        r2 = self._request("GET", "/leaderboard/top?region=KR&limit=5", headers=headers)
        self.assertEqual(r2.status_code, 401)
        self.assertIn("replay", r2.text.lower())

    def test_readiness_and_metrics_endpoints(self):
        r = self._request("GET", "/readiness")
        self.assertEqual(r.status_code, 200)
        payload = r.json()
        self.assertIn("ready", payload)
        self.assertIn("checks", payload)
        self.assertTrue(any(c.get("name") == "db" for c in payload.get("checks", [])))

        m = self._request("GET", "/metrics")
        self.assertEqual(m.status_code, 200)
        self.assertIn("kbbq_players_total", m.text)
        self.assertIn("kbbq_uptime_seconds", m.text)

    def test_ops_alerts_requires_token(self):
        denied = self._request("GET", "/ops/alerts")
        self.assertEqual(denied.status_code, 401)

        allowed = self._request("GET", "/ops/alerts", headers={"X-Ops-Token": os.environ["KBBQ_OPS_TOKEN"]})
        self.assertEqual(allowed.status_code, 200)
        payload = allowed.json()
        self.assertIn("alerts", payload)
        self.assertGreaterEqual(len(payload["alerts"]), 1)

    def test_submit_score_requires_signed_headers(self):
        r = self._request("POST", "/auth/guest", json={"deviceId": "device-test-002"})
        self.assertEqual(r.status_code, 200)
        data = r.json()
        player_id = data["playerId"]
        token = data["token"]

        body = {
            "playerId": player_id,
            "score": 123.4,
            "timestamp": int(time.time()),
            "nonce": "body-nonce-1",
            # Body signature is validated server-side (rounded score).
            "signature": "",
        }

        secret = os.environ["KBBQ_HMAC_SECRET"]
        score_int = int(round(float(body["score"])))
        body["signature"] = hmac_b64(secret, f"{player_id}|{score_int}|{body['timestamp']}")

        raw_body = json.dumps(body, separators=(",", ":"))
        ts = int(time.time())
        nonce = "header-nonce-1"
        headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
            **_sign_headers(secret=secret, player_id=player_id, nonce=nonce, ts=ts, raw_body=raw_body),
        }

        r = self._request("POST", "/leaderboard/submit", headers=headers, content=raw_body)
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

        # Using the same header nonce again should fail.
        r2 = self._request("POST", "/leaderboard/submit", headers=headers, content=raw_body)
        self.assertEqual(r2.status_code, 401)
        self.assertIn("replay", r2.text.lower())

    def test_invalid_clock_skew_env_uses_fallback(self):
        r = self._request("POST", "/auth/guest", json={"deviceId": "device-test-003"})
        self.assertEqual(r.status_code, 200)
        data = r.json()
        player_id = data["playerId"]
        token = data["token"]

        prev_skew = os.environ.get("KBBQ_MAX_CLOCK_SKEW_SECONDS")
        os.environ["KBBQ_MAX_CLOCK_SKEW_SECONDS"] = "not-a-number"
        try:
            secret = os.environ["KBBQ_HMAC_SECRET"]
            ts = int(time.time())
            nonce = "nonce-skew-fallback-1"
            headers = {
                "Authorization": f"Bearer {token}",
                **_sign_headers(secret=secret, player_id=player_id, nonce=nonce, ts=ts, raw_body=""),
            }
            res = self._request("GET", "/leaderboard/top?region=KR&limit=5", headers=headers)
            self.assertEqual(res.status_code, 200)
            self.assertIn("entries", res.json())
        finally:
            if prev_skew is None:
                os.environ.pop("KBBQ_MAX_CLOCK_SKEW_SECONDS", None)
            else:
                os.environ["KBBQ_MAX_CLOCK_SKEW_SECONDS"] = prev_skew

    def test_feedback_relay_requires_endpoint(self):
        r = self._request("POST", "/auth/guest", json={"deviceId": "device-feedback-001"})
        self.assertEqual(r.status_code, 200)
        auth = r.json()
        player_id = auth["playerId"]
        token = auth["token"]

        body = {
            "playerId": player_id,
            "message": "Need more daily mission variety.",
            "email": "player@example.com",
            "channel": "in-game",
            "timestamp": int(time.time()),
            "nonce": "feedback-body-nonce-1",
            "signature": "",
        }
        secret = os.environ["KBBQ_HMAC_SECRET"]
        normalized_message = " ".join(body["message"].split())
        body["signature"] = hmac_b64(secret, f"{player_id}|{body['timestamp']}|{normalized_message}")

        raw_body = json.dumps(body, separators=(",", ":"))
        headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
            **_sign_headers(
                secret=secret,
                player_id=player_id,
                nonce="feedback-header-nonce-1",
                ts=int(time.time()),
                raw_body=raw_body,
            ),
        }

        prev_endpoint = os.environ.pop("KBBQ_FORMSPREE_ENDPOINT", None)
        try:
            res = self._request("POST", "/community/feedback", headers=headers, content=raw_body)
            self.assertEqual(res.status_code, 503)
            self.assertIn("not configured", res.text.lower())
        finally:
            if prev_endpoint is not None:
                os.environ["KBBQ_FORMSPREE_ENDPOINT"] = prev_endpoint

    def test_feedback_relay_success(self):
        r = self._request("POST", "/auth/guest", json={"deviceId": "device-feedback-002"})
        self.assertEqual(r.status_code, 200)
        auth = r.json()
        player_id = auth["playerId"]
        token = auth["token"]

        body = {
            "playerId": player_id,
            "message": "Please add event recap in inbox.",
            "email": "qa@example.com",
            "channel": "in-game",
            "timestamp": int(time.time()),
            "nonce": "feedback-body-nonce-2",
            "signature": "",
        }
        secret = os.environ["KBBQ_HMAC_SECRET"]
        normalized_message = " ".join(body["message"].split())
        body["signature"] = hmac_b64(secret, f"{player_id}|{body['timestamp']}|{normalized_message}")

        raw_body = json.dumps(body, separators=(",", ":"))
        headers = {
            "Authorization": f"Bearer {token}",
            "Content-Type": "application/json",
            **_sign_headers(
                secret=secret,
                player_id=player_id,
                nonce="feedback-header-nonce-2",
                ts=int(time.time()),
                raw_body=raw_body,
            ),
        }

        class _Resp:
            status_code = 200

            @staticmethod
            def json():
                return {"ok": True}

        prev_endpoint = os.environ.get("KBBQ_FORMSPREE_ENDPOINT")
        os.environ["KBBQ_FORMSPREE_ENDPOINT"] = "https://formspree.io/f/mock"
        try:
            with patch("server.app.httpx.post", return_value=_Resp()) as mocked_post:
                res = self._request("POST", "/community/feedback", headers=headers, content=raw_body)
            self.assertEqual(res.status_code, 200)
            self.assertTrue(res.json().get("ok"))
            self.assertEqual(mocked_post.call_count, 1)
        finally:
            if prev_endpoint is None:
                os.environ.pop("KBBQ_FORMSPREE_ENDPOINT", None)
            else:
                os.environ["KBBQ_FORMSPREE_ENDPOINT"] = prev_endpoint

    async def _request_async(self, method: str, url: str, **kwargs) -> httpx.Response:
        transport = httpx.ASGITransport(app=self.app)
        async with httpx.AsyncClient(transport=transport, base_url="http://testserver") as client:
            return await client.request(method, url, **kwargs)

    def _request(self, method: str, url: str, **kwargs) -> httpx.Response:
        return asyncio.run(self._request_async(method, url, **kwargs))


if __name__ == "__main__":
    unittest.main()
