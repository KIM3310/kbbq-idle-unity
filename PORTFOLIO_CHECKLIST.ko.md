# KBBQ Idle 포트폴리오 체크리스트

이 문서는 코드 리뷰어/면접관에게 프로젝트 완성도를 빠르게 보여주기 위한 최소 검증 순서입니다.

## 1) 품질 게이트 실행
```bash
tools/portfolio_quality_gate.sh
```

엄격 모드(모든 게이트 필수):
```bash
STRICT_PORTFOLIO_GATE=1 tools/portfolio_quality_gate.sh
```

통과 기준:
- `sim/` 수식 테스트 통과
- `server/tests` 통과
- Unity EditMode/PlayMode/데이터 검증 통과

## 2) 데모 시나리오 (3~5분)
1. `Assets/Scenes/Main.unity`를 실행하고 초당 수익 증가 확인
2. 업그레이드 구매 후 `Income/sec` 상승 확인
3. `Serve`/`Rush`로 큐 처리와 만족도 변화를 확인
4. Monetization 패널에서 광고 보상/패키지 구매 플로우 확인
5. Leaderboard 패널에서 새로고침(네트워크 off: mock, on: live) 확인

## 3) 기술 포인트 설명(면접 답변용)
- 세이브 무결성: `SaveSystem`의 SHA-256 체크섬 + 백업 슬롯 복구
- 런타임 안정화: Queue Guardian/Auto Rush, 프레임 히치 기반 보호
- 네트워크 보안: HMAC 서명 헤더, nonce 재사용 방지, 토큰 해시 저장
- IAP 검증 경로: 서버 권한형 grant + transaction idempotency

## 4) 주요 파일 맵
- 게임 오케스트레이션: `Assets/Scripts/Core/GameManager.cs`
- 수익화/구매: `Assets/Scripts/Core/MonetizationService.cs`
- 리더보드 UI: `Assets/Scripts/UI/LeaderboardView.cs`
- 네트워크 클라이언트: `Assets/Scripts/Network/`
- 백엔드 API: `server/app.py`, `server/tests/test_api.py`

## 5) 데모 전 점검
- `ApiConfig.asset`에서 기본값은 네트워크 OFF 유지(포트폴리오 안전 기본값)
- 라이브 데모 시에만 `enableNetwork=true`, `baseUrl`, `hmacSecret` 설정
- Unity 버전 `2022.3.62f3` 확인
