# CoT 실행 로그 (요약형)

이 문서는 내부 사고과정을 그대로 노출하지 않고, 실행 단계를 추적 가능한 형태로 기록한다.

## Step 1. 갭 식별
- 실서비스 기준 미충족 영역 확인:
  - 환경별 보안 설정 체계
  - IAP 리시트 검증 신뢰도
  - 서비스 준비상태 진단 엔드포인트
  - 네트워크 장애 재시도 내구성

## Step 2. 서버 보안/운영성 강화
- `server/settings.py` 추가:
  - env/cors/secret/iap mode 파싱
  - readiness 평가 함수
- `server/app.py` 개선:
  - `/readiness` 엔드포인트 추가
  - CORS를 설정 기반으로 적용
  - IAP 검증 시 설정 기반 리시트 검증 호출
- `server/iap_verifier.py` 추가:
  - `mock` 모드
  - `structured` 모드(JSON/base64 JSON)
  - `store` 모드(Apple/Google 검증 연동)
- `server/store_verifier.py` 추가:
  - Apple verifyReceipt 호출
  - Google Play Developer API 호출
- `server/app.py` 개선:
  - `/metrics` 및 `/ops/alerts` 추가

## Step 3. Unity 안정성 강화
- `ApiClientBase`에 재시도(일시적 실패 대응) 추가
- `ApiConfig`에 `maxRetries` 추가
- `MonetizationService`에 외부 리시트 주입형 구매 API 추가

## Step 4. 테스트 보강
- `server/tests/test_api.py`:
  - `/readiness` 테스트 추가
- `server/tests/test_iap_verifier.py`:
  - 모드별 영수증 검증 테스트 추가
- `Assets/Tests/EditMode/MonetizationDefensiveTests.cs`:
  - 리시트 주입형 구매 방어 테스트 추가

## Step 5. 품질 게이트 검증
- server tests 실행
- Unity CI 체크 실행
- strict gate 실행 후 결과 확인

## Step 6. 운영 자동화
- `.github/workflows/backend-deploy.yml` 추가
- `.github/workflows/backend-ops-monitor.yml` 추가
- `tools/deploy_backend.sh`, `tools/check_backend_ops.sh`, `tools/backup_kbbq_db.sh` 추가
