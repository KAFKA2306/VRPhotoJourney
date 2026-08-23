# VRPhotoJourney

Unity Editorで、**明示的なexhibition manifestから固定写真展示packを生成する**ツールです。

利用者PCの任意folderをVRChat runtimeで列挙する仕組みではありません。写真はbuild前にUnity project内へ明示的に用意し、manifestで順序・表示情報・権利状態を指定します。

## Canonical flow

1. [`examples/exhibition.sample.json`](examples/exhibition.sample.json) と同じ形式でmanifestを作る。
2. `ExhibitionManifestPolicy` がpath、形式、rights status、重複checksum、表示順を監査する。
3. Unityの `Tools/Exhibition Pack Generator` から `ExhibitionPackWindow` を開く。
4. `ExhibitionPackGenerator` が受理済み写真からtexture / material / photo prefab / exhibition prefabを生成する。
5. `exhibition-report.json` / `.md` の入力件数、採否理由、checksum、生成asset、要人間確認項目を確認してから展示へ使う。

0枚生成は成功扱いにしません。元画像はrename / delete / overwriteせず、unsupported format、missing source、duplicate checksum等はreason code付きで扱います。

## Current surface

- `ExhibitionPackWindow.cs` — canonical Unity Editor入口
- `ExhibitionManifestPolicy.cs` — manifest / photo入力の監査
- `ExhibitionPackGenerator.cs` — fixed Unity asset / report生成
- `Photo Frame.prefab` — 生成する写真frameのtemplate
- `examples/exhibition.sample.json` — sample manifest
- [`docs/business/vr-photo-exhibition-pack.md`](docs/business/vr-photo-exhibition-pack.md) — 制作範囲、権利境界、有償PoC

## Verification

```bash
dotnet test tests/ExhibitionManifestPolicy.Tests/ExhibitionManifestPolicy.Tests.csproj --configuration Release
```

CIはmanifestの表示順、duplicate checksum、unsupported/missing source、path traversal、rights status、empty pack、report checksumを検証し、退役したruntime slideshow pathがcurrent treeへ戻らないことも確認します。

このCIはUnity Editor APIを実行するものではありません。実際のtexture/material/prefab生成とVRChat内表示は、対象Unity projectで別途確認が必要です。

## Privacy / rights

private photoや顧客写真をこのpublic repositoryへ保存する前提にしません。有償展示では利用権を確認した画像だけを入力し、manifest/reportに必要以上の個人情報を含めないでください。
