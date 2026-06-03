# LeagueOfPhysical-MasterData-Client (com.baegames.lop.masterdata.client)

League of Physical 클라 전용 MasterData (Luban 생성 스키마 + 데이터).

## 책임

- Luban 생성 MasterData (Character/Skin/SkinAsset/Action/Item `.cs`)
- 데이터 (`.bytes` in StreamingAssets)

## Use-side Requirements

- `com.code-philosophy.luban` (UPM git `https://github.com/focus-creative-games/luban_unity.git#v1.2.0`)
- `com.cysharp.unitask`

상세 토폴로지: 사용 측 저장소의 `docs/lop-repo-topology.md` 참조.

## Editing

이 패키지는 *exporter 산출물*이라 직접 편집 금지. 변경하려면 `infrastructure/table/Datas` 수정 + `gen.sh` 재실행.
