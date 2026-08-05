# 작살잡이 프로젝트 규칙

이 문서는 현재 저장소에서 확인한 시스템 구조를 요약한다. 세부 수치와 밸런스는 실제 ScriptableObject, CSV, Prefab 설정을 최종 기준으로 삼는다.

## Stage 구조

- `StageManager`가 스테이지 진행을 관리한다.
- 단계는 `StagePhase`의 `NormalBattle`, `Rest`, `BossBattle`로 구분한다.
- 전투 구성 데이터는 `BattleDatabase`와 `EnemyBattleData`를 사용한다.
- 저장 데이터에는 현재 스테이지, 일반 전투 진행 수, 현재 단계, 보스 진행 순서와 선택된 보스 ID가 포함된다.
- 스테이지 규칙을 수정할 때 `StageManager`, `GameFlowManager`, `BattleDatabase`, 저장/이어하기를 함께 확인한다.

## Card 구조

- 카드 원본 데이터는 `CardData` ScriptableObject로 관리한다.
- 기본 필드는 카드 ID, 이름, 설명, 소유 클래스, 타입, 희귀도, 이미지, 효과 목록이다.
- 카드 효과 한 항목은 `CardEffectData`이며 `CardEffectType`, 대상, 수치, 상태 효과 등의 정보를 가진다.
- 실제 효과 실행은 `CardEffectExecutor`가 담당한다.
- 카드 강화는 원본 설명을 덮어쓰지 않고 런타임 설명과 강화 상태를 사용한다.
- CSV에서 카드를 가져오는 흐름은 `Assets/Scripts/Editor/CardCSVImporter.cs`를 기준으로 유지한다.
- 카드 ID는 저장 복원과 데이터 조회에 사용되므로 이미 배포된 ID를 가볍게 변경하지 않는다.

## Deck 구조

- `DeckManager`가 보유 덱, 뽑기 더미, 버림 더미 등 카드 흐름을 관리한다.
- 현재 파일명은 역사적 이유로 `DackManager.cs`이지만 클래스명은 `DeckManager`다. 별도 정리 작업 없이 파일명을 바꾸지 않는다.
- `HandManager`가 손패와 카드 선택/사용 흐름을 담당한다.
- 시작 덱은 `StartingDeckDatabase`, `StartingDeckEntry`, `StartingDeckUI`와 연결된다.
- 런타임 카드 상태가 원본 `CardData` 자산을 오염시키지 않도록 카드 복제 흐름을 유지한다.
- 이어하기 데이터는 카드 ID, 강화 여부, 첫 손패 순서, 남은 뽑기 더미 순서를 보존한다.

## Enemy 구조

- `EnemySpawner`가 전투 데이터에 맞춰 적을 생성하고 `Enemy`가 개별 적 상태를 관리한다.
- 적 행동은 `EnemyPatternCSVLoader`, `EnemyPatternController`, `EnemyPatternExecutor`, `EnemyPatternConditionChecker`로 분리되어 있다.
- `EnemyPatternData`는 적 ID, 패턴 턴, 실행 순서, 행동, 값, 반복 횟수, 대상, 상태 효과, 지속시간, 영구 여부, 조건을 가진다.
- 같은 턴에 여러 행동이 있으면 `executionOrder` 순서를 보존한다.
- 행동/조건 enum이나 CSV 열을 바꿀 때 로더, 실행기, Intent UI를 함께 수정한다.
- 적 의도 표시는 `EnemyIntentUI`와 `EnemyIntentIconDatabase`를 사용한다.

## Summon 규칙

- 소환 유닛은 코드에서 `Crew`로 표현하고 `CrewManager`가 생성, 순서, 성장, 제거를 관리한다.
- 기본 최대 소환 수는 Inspector의 `maxCrewCount`이며 현재 코드 기본값은 3이다.
- 소환 가능 여부는 최대 수와 등록된 `spawnPoints` 수를 모두 만족해야 한다.
- 새 소환은 현재 목록의 다음 빈 순서 위치에 생성된다.
- 소환 유닛이 제거되면 남은 유닛을 앞쪽 Spawn Point부터 재배치하며 기존 순서를 유지한다.
- 카드의 `Summon`/`SummonCrew` 효과는 `CardEffectExecutor`를 통해 `CrewManager`에 전달한다.
- 소환 규칙 변경 시 `Crew`, `CrewManager`, 카드 효과, 대상 타입(`Undead` 계열), UI를 함께 확인한다.

## Status 규칙

- 상태 효과 종류는 `StatusEffectType`이 단일 기준이다.
- `StatusEffectHandler`가 추가, 중첩, 제거, 지속시간 감소와 변경 이벤트를 관리한다.
- 플레이어와 적 모두 같은 Handler 구조를 사용한다.
- UI 아이콘은 `StatusEffectIconDatabase`, `StatusEffectIconData`, 플레이어/적 Status UI에서 표시한다.
- 상태 효과 추가 시 최소한 enum, 적용 로직, 턴 감소 규칙, 아이콘 데이터, 툴팁/표시, 적 Intent 분류를 점검한다.
- 상태별 중첩 및 영구 지속 예외는 `StatusEffectHandler`의 현재 구현을 기준으로 하며 임의로 일반화하지 않는다.

## CSV 규칙

- 카드 CSV는 Editor importer를 통해 `CardData` 자산으로 반영한다.
- 적 패턴 CSV는 전용 Loader를 통해 런타임 패턴 데이터로 읽는다.
- CSV 헤더, enum 문자열, ID는 코드와 정확히 일치해야 한다.
- 열 추가/삭제 시 기존 CSV와의 하위 호환 또는 마이그레이션 방법을 함께 제공한다.
- Import 전후 한글 인코딩과 쉼표/줄바꿈이 포함된 설명 필드를 확인한다.

## Save / Continue 규칙

- `SaveManager`, `GameSaveData`, `ContinueLoadContext`, 타이틀/덱/시작 UI가 이어하기 흐름을 구성한다.
- 현재 저장 버전 기본값은 3이다.
- 이어하기는 진행 중 전투 상태를 그대로 저장하는 방식이 아니라, 저장된 전투 시작 상태와 카드 순서를 복원해 전투를 처음부터 재시작하는 구조다.
- 저장 필드 추가/의미 변경 시 구버전 저장 파일의 기본값과 복원 실패 처리를 정의한다.

## Audio / Shortcut 규칙

- BGM은 `BGMManager`, 효과음은 `SFXManager`, 볼륨 설정은 `AudioSettingsManager`가 담당한다.
- 전투 단축키는 `BattleShortcutController`, 클래스 선택 단축키는 `ClassSelectShortcutController`에서 관리한다.
- 현재 전투 단축키에는 손패 숫자 선택, `E` 보조/확정, `D/A/S` 덱 목록 전환이 포함된다. Pause 상태와 시작 덱 확인 화면의 입력 차단을 유지한다.

## 확인 필요

- 스테이지별 정확한 전투 횟수와 보스 선택 규칙
- 모든 상태 효과의 기획 정의 및 중첩 우선순위
- 카드 CSV와 적 패턴 CSV의 공식 컬럼 명세
- 소환 유닛의 기획상 공식 명칭(선원/언데드 등)
