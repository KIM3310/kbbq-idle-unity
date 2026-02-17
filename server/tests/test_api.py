import json
import os
import tempfile
import time
import unittest

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

        from fastapi.testclient import TestClient
        from server.app import app

        cls.client = TestClient(app)

    @classmethod
    def tearDownClass(cls):
        try:
            cls._tmp.cleanup()
        except Exception:
            pass

    def test_root_and_health(self):
        r = self.client.get("/")
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

        r = self.client.get("/health")
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

    def test_auth_then_signed_leaderboard_and_replay_protection(self):
        # 1) Guest auth
        r = self.client.post("/auth/guest", json={"deviceId": "device-test-001"})
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
        r = self.client.get("/leaderboard/top?region=KR&limit=5", headers=headers)
        self.assertEqual(r.status_code, 200)
        self.assertIn("entries", r.json())

        # 3) Reusing the same nonce should fail (replay protection)
        r2 = self.client.get("/leaderboard/top?region=KR&limit=5", headers=headers)
        self.assertEqual(r2.status_code, 401)
        self.assertIn("replay", r2.text.lower())

    def test_readiness_and_metrics_endpoints(self):
        r = self.client.get("/readiness")
        self.assertEqual(r.status_code, 200)
        payload = r.json()
        self.assertIn("ready", payload)
        self.assertIn("checks", payload)
        self.assertTrue(any(c.get("name") == "db" for c in payload.get("checks", [])))

        m = self.client.get("/metrics")
        self.assertEqual(m.status_code, 200)
        self.assertIn("kbbq_players_total", m.text)
        self.assertIn("kbbq_uptime_seconds", m.text)

    def test_ops_alerts_requires_token(self):
        denied = self.client.get("/ops/alerts")
        self.assertEqual(denied.status_code, 401)

        allowed = self.client.get("/ops/alerts", headers={"X-Ops-Token": os.environ["KBBQ_OPS_TOKEN"]})
        self.assertEqual(allowed.status_code, 200)
        payload = allowed.json()
        self.assertIn("alerts", payload)
        self.assertGreaterEqual(len(payload["alerts"]), 1)

    def test_submit_score_requires_signed_headers(self):
        r = self.client.post("/auth/guest", json={"deviceId": "device-test-002"})
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

        r = self.client.post("/leaderboard/submit", headers=headers, content=raw_body)
        self.assertEqual(r.status_code, 200)
        self.assertTrue(r.json().get("ok"))

        # Using the same header nonce again should fail.
        r2 = self.client.post("/leaderboard/submit", headers=headers, content=raw_body)
        self.assertEqual(r2.status_code, 401)
        self.assertIn("replay", r2.text.lower())


if __name__ == "__main__":
    unittest.main()
