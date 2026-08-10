# 완료된 이전 작업

## 작업명

Assets/Scripts 폴더 구조 정리 1단계 — Managers 역할별 분류

## 목표

- `Assets/Scripts/Managers`에 혼재한 스크립트를 역할별 기존 영역으로 이동한다.
- 스크립트와 `.meta`를 함께 이동하여 Unity GUID 참조를 보존한다.
- 코드, 클래스명, namespace와 공개 API는 변경하지 않는다.

## 현재 상태

완료 — 2026-08-06

## 변경 범위

- `Assets/Scripts/Core`
- `Assets/Scripts/Audio`
- `Assets/Scripts/Battle/Managers`
- `Assets/Scripts/Cards/Managers`
- `Assets/Scripts/Player/Managers`
- `Assets/Scripts/Scenes/Managers`

## 검증

- 16개 스크립트와 각 `.meta` 파일의 이동을 확인했다.
- 이동 전후 스크립트 GUID가 모두 동일함을 확인했다.
- Unity 6000.0.78f1에서 16개 이동을 인식하고 `Assembly-CSharp`와 `Assembly-CSharp-Editor` 컴파일 및 도메인 재로드가 완료됐다.
- `Editor.log`에서 C# 컴파일 오류와 Missing Script 관련 오류가 없음을 확인했다.
- 이동한 스크립트 15개의 기존 Scene 직렬화 참조가 유지됨을 확인했다. `GameFlowManager`는 직렬화 참조가 없다.
- Play Mode 기능 회귀 테스트는 실행하지 않았다.

## 다음 작업

사용자가 `다음`이라고 하면 `Assets/Scripts/UI` 정리 범위를 분석한다. 승인 전에는 추가 이동을 진행하지 않는다.

# 현재 작업

## 작업명

카드 더미 이동 VFX

## 목표

1. 턴 종료 시 보존되지 않은 손패 카드가 버림 덱 위치로 이동한다.
2. 기존 `CardUI` 오브젝트를 이동하고 회전시킨다.
3. 드로우 파일이 비었을 때 버림 파일을 재셔플한다.
4. 버림 카드가 소용돌이에 흡수되는 연출을 재생한다.
5. 소용돌이가 드로우 덱 위치로 이동한다.
6. 물기둥과 비말을 재생한다.
7. 연출 후 실제 드로우 파일을 재생성한다.

## 구현 원칙

- 한 번에 한 단계만 진행한다.
- 사용자가 `다음`이라고 하기 전에는 다음 단계를 구현하지 않는다.
- 외부 애니메이션 플러그인과 DOTween을 사용하지 않는다.
- Unity 기본 Coroutine, Particle System과 Transform 이동을 우선 사용한다.
- `Assets/NamuFX`의 외부 원본 Prefab과 Material은 직접 수정하지 않는다.
- 프로젝트용 복제본은 `Assets/Art/VFX/Water`에서 관리한다.
- 기존 카드, 덱, 손패, 보존 카드 및 턴 종료 로직을 임의로 변경하지 않는다.

## 단계

1. `VFX_DiscardArrivalSplash` 튜닝
2. 버림 덱 위치 `Transform` 확보
3. VFX 테스트 재생 메서드 구현
4. 카드 1장 이동 연출
5. 여러 카드 순차 이동
6. 마지막 카드 도착 시 Splash 재생
7. 실제 턴 종료 로직과 연결
8. 재셔플 연출 설계
9. 재셔플 카드 이동
10. 물기둥 재생 및 드로우 파일 재생성
11. 전체 회귀 테스트

## 현재 단계

7단계 — 실제 턴 종료 로직과 연결

## 현재 상태

7단계 완료 — 사용자 Unity Play Mode 확인 완료

## 확인된 사실

- 프로젝트용 Prefab은 `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`에 존재한다.
- 외부 원본은 `Assets/NamuFX/StylizedWaterEffects/Prefabs/Water_Splash_A.prefab`이다.
- 프로젝트용 Prefab은 원본과 비교했을 때 루트 이름만 다르고 Particle System 설정은 동일하다.
- 루트와 여섯 자식 Particle System은 `Looping`이 꺼져 있고 `Play On Awake`가 켜져 있다.
- 프로젝트용 도착 Prefab은 세로 물기둥을 끄고 낮은 물보라·링·버블 중심으로 조정했다.
- Battle Scene의 `BattleCanvas/BattlePanel/Discard deck`은 `RectTransform`이고 손패 부모와 같은 `BattlePanel` 아래에 있다.
- `HandManager`의 `discardPileTarget`에 `Discard deck` RectTransform이 연결되어 있다.
- 카드 UI가 순차적으로 물빛 변환 후 버림 덱으로 가속 이동하는 테스트를 사용자가 확인했다.
- 이동 카드는 원본의 25% 크기를 유지하고 프로젝트용 물 VFX가 카드 위치를 따라간다.
- 보존 확정 시 선택한 카드 UI를 제외한 나머지 카드에 연출을 실행하도록 연결했다.
- 연출 완료 후 기존 `DiscardUnpreservedCards`, Jinx 초기화와 턴 전환을 실행한다.
- 실제 보존 확정 흐름에서 카드 이동, 버림 처리와 턴 전환이 정상 동작함을 사용자가 Play Mode에서 확인했다.
- 보존 유무와 손패 수별 모든 경계 조건의 개별 검증 여부는 확인되지 않았다.

## 현재 단계 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 현재 단계 금지 범위

- `Assets/NamuFX` 외부 원본 수정
- 기존 보존 카드 판정과 버림 덱 데이터 규칙 변경
- `BattleScene`의 승인된 VFX Inspector 참조 외 Scene 수정
- 다른 Prefab 수정
- 다른 카드 더미 VFX 수정
- 재셔플 연출 구현

## 현재 단계 완료 조건

- 보존 카드는 이동 연출 대상에서 제외된다.
- 나머지 카드 연출이 끝난 후 실제 버림 덱 데이터가 갱신된다.
- 연출 중 중복 확정과 카드 선택이 차단된다.
- 보존 카드 없음과 버릴 카드 없음 경로가 정상 종료된다.
- 연출 후 적 턴이 한 번만 시작된다.
- Unity 컴파일 오류와 Missing Reference가 없다.

## 다음 작업

사용자가 `다음`이라고 하면 8단계 재셔플 연출을 설계한다. 승인 전에는 재셔플 코드와 VFX를 수정하지 않는다.

# 현재 작업 변경

## 작업명

뽑을 더미에서 손패로 순차 드로우 연출

## 목표

- 새로 뽑히는 카드가 `Decktodrawfrom`에서 각 카드의 최종 손패 위치로 이동한다.
- 여러 장은 오른쪽 손패 자리부터 왼쪽 방향으로 한 장씩 이동한다.
- 드로우 이동 시간은 `0.27초`로, 버림 카드 `0.54초`보다 2배 빠르게 재생한다.
- 기존 드로우, 덱 재생성, Jinx와 턴 시작 규칙을 보존한다.

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 최초 4장과 새 턴 부족분이 오른쪽 자리부터 한 장씩 이동한다.
- 카드 효과 드로우에도 같은 연출이 적용된다.
- 연출 중 카드 선택과 턴 종료가 차단된다.
- Jinx 상태가 드로우 완료 후 정상 표시된다.
- Unity 컴파일 오류와 Missing Reference가 없다.

# 현재 작업 변경

## 작업명

보존 모드 UX 1단계 — ESC 취소

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scripts/Core/PauseManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 보존 모드에서 ESC를 누르면 선택을 해제하고 일반 전투 상태로 복귀한다.
- 같은 ESC 입력으로 Pause 패널이 열리지 않는다.
- 기존 보존 카드 데이터와 턴 상태는 변경되지 않는다.

# 현재 작업 변경

## 작업명

보존 모드 UX 2단계 — 손패 제외 화면 70% 암전

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 완료 조건

- 보존 모드 진입 시 손패를 제외한 화면이 검정 70% 불투명도로 어두워진다.
- ESC 취소와 보존 확정 시 암전이 해제된다.
- 암전 UI가 카드 및 단축키 입력을 막지 않는다.
- 암전 Canvas는 Sorting Order `100`, 손패 Canvas는 `101`로 고정하여 다른 전투 UI와 무관하게 암전과 손패 표시 순서를 보장한다.
- 보존 확정 버튼도 Canvas Sorting Order `101`로 표시하여 암전에서 제외한다.
- 보존 확정 버튼은 보존 모드에서만 표시하고 시작 덱·일반 전투·취소·확정 상태에서는 숨긴다.

# 현재 작업 변경

## 작업명

보존 모드 UX 3단계 — 선택 카드 중앙 확대

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 완료 조건

- 보존 카드 선택 시 드로우와 동일한 `0.27초`에 화면 중앙으로 이동한다.
- 선택 카드는 원본의 `1.4배`로 확대되고 회전이 제거된다.
- 재선택·다른 카드 선택·ESC 취소·보존 확정 시 원래 손패 배치로 복원된다.

# 현재 작업 변경

## 작업명

버프/디버프 아이콘 Hover 설명 표시

## 현재 상태

구현 및 사용자 Unity Play Mode 확인 완료

## 수정 대상

- `Assets/Scripts/UI/EnemyStatusIconUI.cs`
- `Assets/Prefabs/Enemy/EnemyStatusIcon.prefab`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 플레이어와 적 상태 효과 아이콘에 마우스를 올리면 이름, 설명, 현재 수치와 지속 정보가 표시된다.
- 마우스가 벗어나거나 아이콘이 제거되면 툴팁이 숨겨진다.
- 툴팁이 화면 가장자리 밖으로 나가지 않는다.
- 툴팁은 다른 UI 입력을 가로막지 않는다.
- 버프·디버프 이름과 설명은 기획 정리표의 공식 한글 표기를 사용한다.
- 툴팁은 상태 아이콘과 동일한 `Pretendard-Regular SDF` 폰트와 Material을 사용한다.
- 사용자 확인 결과 툴팁 한글명과 폰트가 정상 표시되며 Console 오류가 없다.

# 현재 작업 변경

## 작업명

버림 덱 셔플 VFX 1단계 — 소용돌이와 난류 경로 테스트

## 현재 상태

구현 완료 — Unity Play Mode 확인 필요

## 수정 대상

- `Assets/Scripts/Cards/Managers/HandManager.cs`
- `Assets/Scenes/PlayScene/BattleScene.unity`
- `CURRENT_TASK.md`
- `Docs/DEVLOG.md`

## 완료 조건

- 버림 덱 위치에서 소용돌이 VFX가 먼저 재생된다.
- 이후 난류 VFX가 버림 덱에서 뽑을 덱 위치로 이동한다.
- 테스트 중 실제 버림 덱, 뽑을 덱과 손패 데이터는 변경되지 않는다.
- 기존 Base VFX 프리팹은 수정하지 않는다.
- 작은 물살 3개가 서로 다른 곡선으로 버림 덱에서 뽑을 덱까지 순차 이동한다.
- 이동 물살은 작은 물 입자 꼬리를 남기고, 마지막 도착 시 낮은 물결 Splash가 재생된다.
- 물 폭탄과 높은 물기둥은 사용하지 않는다.
- 기존 범용 물 VFX 테스트 대신 전용 `VFX_DeckShuffleTransfer` 프리팹을 사용한다.
- 응축 파동, 카드 잔상 교차 셔플, 이중 S자 물빛 리본, 도착 재구성의 네 단계로 재생한다.
- 전용 연출 테스트에서도 실제 덱 데이터는 변경하지 않는다.

## 2026-08-10 셔플 이동 VFX 물 표현 조정

- 전용 물 Trail Material을 이동 물줄기에 적용한다.
- 버림 덱에서 먼저 위로 솟은 후 뽑을 덱으로 흐르는 2단계 이동 경로를 사용한다.
- 실제 덱 셔플 로직 연결은 이번 단계에서 변경하지 않는다.
- Unity Play Mode 시각 확인이 필요하다.
- 상승 높이를 6배로 확대하고 초반 40% 동안 위로 상승한다.
- 손패 위쪽 높이를 유지하다가 뽑을 덱 가까이에서 하강한다.
- 이동 물줄기 주변에 작은 물방울을 함께 표시한다.
- 이동 시간을 0.65초로 조정하고 상승 높이를 7.5배로 확대한다.
- 이동 경로는 유지하고 물줄기, 파동, 물방울과 카드 잔상 크기를 1.35배로 확대한다.
- 전체 색상을 채도 낮은 딥 블루·청록 계열로 낮춰 어두운 전투 분위기에 맞춘다.
