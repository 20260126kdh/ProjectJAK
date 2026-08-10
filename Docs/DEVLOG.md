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

## 2026-08-10 — 보존 모드 ESC 취소

- ESC 입력을 전담하는 `PauseManager`에서 일반 Pause보다 보존 모드 취소를 우선 처리
- 보존 선택 표시를 해제하고 턴 종료 버튼을 복원하되 보존 카드 데이터와 턴 진행은 유지
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scripts/Core/PauseManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `PauseManager.handManager`에 BattleScene `HandManager` 연결
- 검증: 사용자 Unity Play Mode에서 보존 취소, 선택 해제, Pause 미진입과 재진입 확인 완료

## 2026-08-10 — 보존 모드 화면 70% 암전

- BattleCanvas 전체를 덮는 검정 70% 불투명도의 `PreserveDimOverlay` 추가
- `HandCardParent`를 별도 Canvas로 정렬하여 암전 위에 손패만 표시
- 암전 밝기 차이가 표시되지 않던 Canvas 정렬 문제를 막기 위해 Overlay Order `100`, 손패 Order `101`로 렌더링 순서 고정
- `PreserveDimOverlay` Image의 잘못된 스크립트 GUID로 Missing Script가 발생한 원인을 확인하고 Unity UI Image GUID로 수정
- 보존 확정 버튼에 Canvas·GraphicRaycaster를 추가하고 Order `101`로 설정하여 손패와 함께 암전에서 제외
- `EndTurnButton`을 보존 확정 버튼으로 잘못 판단한 설정을 복구하고, `ConfirmPreserveCard`가 연결된 실제 `PresrveConfirmButton`으로 Canvas·GraphicRaycaster 이동
- `HandManager`uc5d0 실제 보존 확정 버튼 참조를 추가하고 보존 모드 진입 중에만 표시하도록 상태 연결

## 2026-08-10 — 보존 선택 카드 중앙 확대

- 보존 카드를 드로우와 동일한 `0.27초`에 화면 중앙으로 이동하고 `1.4배`로 확대
- 중앙 표시 시 카드 회전을 제거하고 손패 최상단에 표시
- 재선택, 다른 카드 선택, ESC 취소, 보존 확정과 전투 초기화 경로에서 원래 위치·회전·크기·형제 순서 복원
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`
- Inspector/데이터 변경: `preserveFocusedCardScale` 기본값 `1.4`
- 검증: Unity 컴파일 및 Play Mode 확인 필요
- 버프·디버프 정리표를 기준으로 모든 현재 상태 효과 enum의 한글명과 툴팁 설명을 동기화
- 문서의 `ProfandHalo`는 실제 enum `ProfanedHalo`에 `장송의 가호`로 연결하고, enum에 없는 EOL/EOD는 제외
- 툴팁 폰트를 상태 아이콘의 `Pretendard-Regular SDF`와 동일하게 연결하고 TMP 구형 줄바꿈 API 경고 제거
- 검증: 사용자 Unity Play Mode에서 툴팁 표시와 폰트 정상 동작 및 Console 오류 없음 확인 완료
- 보존 진입, ESC 취소, 보존 확정과 전투 초기화 경로에 암전 표시 상태 연결
- 관련 경로: `Assets/Scripts/Cards/Managers/HandManager.cs`, `Assets/Scenes/PlayScene/BattleScene.unity`
- Inspector/데이터 변경: `HandManager.preserveDimOverlay` 연결, HandCardParent Canvas·GraphicRaycaster 추가
- 검증: 사용자 Unity Play Mode에서 시작 덱 표시, 보존 모드 70% 암전, 손패·보존 확정 버튼 제외와 버튼 상태 전환 확인 완료

```text
## YYYY-MM-DD — 기능명

- 변경 내용
- 관련 경로: Assets/...
- Inspector/데이터 변경: ...
- 검증: ...
- 남은 작업: ...
```

## 2026-08-10 — 상태 효과 아이콘 Hover 툴팁

- 공용 `EnemyStatusIconUI`에 마우스 진입·이탈 처리를 추가해 플레이어와 적 상태 아이콘 모두에 툴팁 표시
- 상태 효과 이름, 실제 로직 기준 설명, 현재 수치와 지속 정보를 표시
- 화면 경계 위치 보정과 Raycast 비활성화로 화면 이탈과 입력 방해를 방지
- 관련 경로: `Assets/Scripts/UI/EnemyStatusIconUI.cs`, `Assets/Prefabs/Enemy/EnemyStatusIcon.prefab`
- 검증: Unity 컴파일 및 Play Mode 확인 필요
