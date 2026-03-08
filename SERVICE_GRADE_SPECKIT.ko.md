# kbbq-idle-unity Service-Grade SPECKIT

Last updated: 2026-03-08

## S - Scope
- 대상: Unity WebGL idle / tycoon game
- baseline 목표: gameplay loop, economy balance, deployability를 서비스 수준으로 정리

## P - Product Thesis
- 이 repo는 단순 프로토타입이 아니라 `playable idle product slice`로 보여야 한다.
- core loop와 WebGL delivery readiness가 핵심이다.

## E - Execution
- onboarding -> progression -> reward loop를 README와 build artifacts로 설명
- WebGL performance 및 scene stability를 baseline 품질 기준으로 유지
- 기존 workflow와 build proof를 계속 유지
- backend `review-pack`과 Unity perf overlay를 연결해 gameplay/economy/monetization posture를 reviewer가 즉시 읽게 만든다.

## C - Criteria
- build/workflow green
- README에서 core loop와 target platform이 즉시 이해됨
- economy/state management가 흔들리지 않음
- `/health`, `/meta`, `/review-pack`, in-game overlay가 같은 계약 언어를 공유함

## K - Keep
- game feel 중심 접근
- delivery-ready WebGL posture

## I - Improve
- gameplay GIF / screenshot pack 추가
- balance notes와 telemetry dashboard 강화
- WebGL reviewer capture와 perf overlay screenshot 추가

## T - Trace
- `README.md`
- `Assets/`
- `server/`
- `ProjectSettings/`
- `.github/workflows/`
