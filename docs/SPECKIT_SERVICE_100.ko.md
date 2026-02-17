# SPECKIT: KBBQ Idle 실서비스 100% 달성 명세

## 1. 목표
- 포트폴리오 품질을 넘어, 운영 가능한 서비스 기준(보안/내구성/운영성)으로 시스템을 고도화한다.
- 최소 목표:
  - 결제 검증 경로 강화
  - 런타임 readiness 진단 가능
  - 네트워크 장애 시 재시도/복원력 확보
  - 품질 게이트로 지속 검증 가능

## 2. 비목표(이번 단계)
- Apple/Google 실결제 검증 API 완전 연동
- 운영 DB(PostgreSQL 등) 이관
- 클라우드 인프라(IaC, WAF, CDN) 구축

## 3. 요구사항 (MUST)
1. 서버 설정을 환경별로 분리하고, prod 안전성 체크가 가능해야 한다.
2. IAP 영수증 검증 모드를 `mock/structured`로 분리해야 한다.
3. `/readiness` 엔드포인트가 핵심 상태를 반환해야 한다.
4. Unity HTTP 클라이언트는 일시적 네트워크 장애에서 재시도해야 한다.
5. 테스트가 신규 경로(리시트 검증/준비상태)를 커버해야 한다.

## 4. 수용 기준 (Acceptance Criteria)
- `STRICT_PORTFOLIO_GATE=1 tools/portfolio_quality_gate.sh` 통과.
- `server/tests` 통과.
- Unity EditMode/PlayMode/Validator 통과.
- `/readiness` 응답에 최소 체크 항목 포함:
  - `db_writable`
  - `hmac_secret_configured`
  - `token_salt_configured`
  - `iap_mode_safe_for_env`
  - `cors_restricted_in_prod`
  - `docs_hidden_in_prod`

## 5. 설계 결정
- 설정 계층:
  - `server/settings.py`에서 환경변수 파싱 및 readiness 계산.
- IAP 검증 계층:
  - `server/iap_verifier.py`에서 영수증 파싱(JSON/base64 JSON) 및 정합성 검증.
- API 계층:
  - `server/app.py`는 요청 인증 + 서명 검증 + 리시트 검증 + idempotency 순서로 처리.
- Unity 계층:
  - `ApiClientBase`에서 상태코드 `429/5xx` 및 연결 오류 재시도.
  - `MonetizationService`는 외부 리시트 주입형 API를 제공하여 SDK 연동 포인트 확보.

## 6. 이후 확장 항목
1. 실스토어 검증 어댑터(Apple/Google) 추가
2. 운영 DB 전환(SQLite → Postgres)
3. 운영관측(알람/지표/트레이스) 추가
4. 결제 취소/환불 동기화 배치 추가
