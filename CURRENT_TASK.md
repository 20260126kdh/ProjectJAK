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

1단계 — `VFX_DiscardArrivalSplash` 튜닝 준비

## 현재 상태

진행 중

## 확인된 사실

- 프로젝트용 Prefab은 `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`에 존재한다.
- 외부 원본은 `Assets/NamuFX/StylizedWaterEffects/Prefabs/Water_Splash_A.prefab`이다.
- 프로젝트용 Prefab은 원본과 비교했을 때 루트 이름만 다르고 Particle System 설정은 동일하다.
- 루트와 여섯 자식 Particle System은 `Looping`이 꺼져 있고 `Play On Awake`가 켜져 있다.
- 실제 턴 종료 코드와 VFX는 아직 연결되지 않았다.
- 실제 화면에서의 크기, 위치, Sorting Layer와 재생 결과는 확인되지 않았다.

## 현재 단계 수정 대상

- `Assets/Art/VFX/Water/VFX_DiscardArrivalSplash.prefab`

## 현재 단계 금지 범위

- `Assets/NamuFX` 외부 원본 수정
- 카드, 덱, 손패와 턴 종료 코드 수정
- Scene과 다른 Prefab 수정
- 다른 카드 더미 VFX 수정
- 재셔플 연출 구현

## 현재 단계 완료 조건

- Battle Scene의 Canvas, 카메라, Sorting Layer와 버림 덱 표시 환경을 확인한다.
- 프로젝트용 Prefab만 조정한다.
- Unity Play Mode에서 도착 Splash의 크기와 가시성을 사용자가 확인한다.
- 확인 전에는 완료로 기록하지 않는다.

## 다음 작업

Battle Scene의 실제 표시 환경을 분석한 뒤 `VFX_DiscardArrivalSplash`의 튜닝 항목과 변경값을 제시한다. 사용자 승인 전에는 Prefab을 수정하지 않는다.
