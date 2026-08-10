# 개발 로그

기능을 완료할 때 날짜, 변경 내용, 관련 경로, 검증 결과를 기록한다. 아래 초기 목록은 2026-08-06 저장소에서 코드와 자산의 존재를 확인해 작성했으며, 최초 구현일은 Git 커밋 메시지만으로 확정할 수 없어 별도로 표기하지 않았다.

## 2026-08-06 — 프로젝트 문서화 시작

- 루트 `AGENTS.md` 추가
- 프로젝트/코딩/게임 디자인/개발 로그 문서 추가
- Unity 버전과 현재 주요 시스템 구조 정리
- 검증: 파일 및 주요 코드 구조 확인

## 구현 확인된 기능 (최초 구현일 확인 필요)

- 카드 데이터, 카드 효과 실행, 강화, CSV Import
- 덱/손패/뽑기·버리기 흐름과 시작 덱 UI
- 적 생성, CSV 기반 적 패턴, 조건 검사와 Intent UI
- 플레이어/적 상태 효과와 상태 아이콘 UI
- 선원(Crew) 소환, 체력, 공격, 회복과 희생 효과
- 스테이지 일반 전투/휴식/보스 진행
- 전투 보상과 카드 강화 UI
- BGM, SFX, 오디오 설정
- 저장, 타이틀 이어하기, 전투 시작 상태 및 카드 순서 복원
- 전투/클래스 선택 단축키와 일시정지
- Spine Runtime과 애니메이션 컨트롤러
- VFX 자산 및 작업 구조

## 과거 이력 후보 (사용자 확인 필요)

다음 날짜는 기존 대화에서 제시된 예시이므로 확인 후 확정 이력으로 이동한다.

- 2026-07-08: Spine 적용
- 2026-07-15: Reward Panel
- 2026-07-28: Intent Icon
- 2026-08-05: VFX 작업 시작

## 2026-08-06 — Scripts Managers 역할별 분류

- `Managers`에 혼재한 16개 스크립트를 Core, Audio, Battle, Cards, Player, Scenes 영역으로 이동
- 관련 경로: `Assets/Scripts/Core`, `Audio`, `Battle/Managers`, `Cards/Managers`, `Player/Managers`, `Scenes/Managers`
- Inspector/데이터 변경: 없음, 기존 `.meta`와 GUID 유지
- 검증: 이동 전후 GUID·내용 해시 일치, 누락·고아 `.meta` 없음, Unity 6000.0.78f1 컴파일·도메인 재로드 성공, C# 및 Missing Script 오류 없음
- 남은 작업: Play Mode 기능 회귀 확인

## 2026-08-06 — 버림 도착 물보라 튜닝 및 위치 참조 확보

- `VFX_DiscardArrivalSplash` 루트 Scale을 `0.45`로 조정하고 사용자 화면 확인 완료
- `HandManager`에 버림 더미 도착 위치용 `RectTransform` 필드 추가
- Battle Scene의 기존 `Discard deck` RectTransform을 Inspector 참조로 연결
- 관련 경로: `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`, `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- 검증: 직렬화 참조 일치, Unity 컴파일·도메인 재로드·Battle Scene 재임포트 성공, Missing Reference 오류 없음
- 남은 작업: VFX 테스트 재생 메서드 구현

## 2026-08-07 — 보존 확정 버림 카드 이동 연출 연결

- 보존하지 않은 손패 카드가 순차적으로 물빛 변환 후 버림 덱으로 가속 이동하는 연출 구현
- 선택한 보존 카드 UI를 연출에서 제외하고 완료 후 기존 버림 데이터와 턴 전환 흐름에 연결
- 연출 중 중복 확정과 카드 선택을 차단하고 설정 누락 시 기존 턴 처리를 계속하는 예외 처리 추가
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`, `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`
- 검증: 단독 이동 연출과 실제 보존 확정 흐름에서 카드 이동·버림 처리·턴 전환이 정상 동작함을 사용자 Play Mode에서 확인
- 남은 작업: 보존 유무·손패 수별 경계 조건 회귀 테스트, 재셔플 연출 설계

## 기록 형식

## 2026-08-10 — 손패 버림 덱 이동 속도 조정

- 손패 카드가 버림 덱까지 이동하는 시간을 `0.7초`에서 `0.54초`로 단축하여 이동 속도를 약 30% 높임
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`
- Inspector/데이터 변경 없음
- 검증: 사용자 Unity Play Mode 확인 완료

## 2026-08-10 — 뽑을 더미에서 손패로 순차 드로우 연출

- 새로 뽑은 카드 UI를 `Decktodrawfrom`에서 최종 손패 위치로 한 장씩 이동하도록 연결
- 카드 데이터와 최종 손패 순서는 유지하고 오른쪽 손패 자리부터 왼쪽 방향으로 이동 연출
- 드로우 이동 시간을 `0.27초`로 분리하여 버림 카드 `0.54초`보다 2배 빠르게 재생하고 연출 중 카드 선택과 턴 종료를 차단
- 턴 시작 Jinx 표시는 드로우 연출 완료 후 적용되도록 연결
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `HandManager.drawPileTarget`에 `Decktodrawfrom` RectTransform 연결
- 검증: 사용자 Unity Play Mode에서 드로우 속도·순서·동작 확인 완료

## 2026-08-10 — TEC_SKL_006 소멸 데이터 동기화

- `CardEffects.csv`의 `Exit` 설정에 맞춰 `TEC_SKL_006.asset` 두 번째 효과를 `Toxic`에서 `Exit`으로 동기화
- 관련 경로: `Assets/Data/CSV/CardEffects.csv`, `Assets/Data/ScriptableObjects/Cards/TEC_SKL_006.asset`
- Inspector/데이터 변경: `statusEffectType` enum 직렬화 값 `23` → `24`
- 검증: CSV와 ScriptableObject 일치, Unity 오류 로그 0건, 사용자 Play Mode 소멸 동작 확인 완료

```text
## YYYY-MM-DD — 기능명

- 변경 내용
- 관련 경로: Assets/...
- Inspector/데이터 변경: ...
- 검증: ...
- 남은 작업: ...
```
