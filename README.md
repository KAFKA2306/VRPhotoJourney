# VRPhotoJourney

Unity Editorで写真slideshowと固定展示assetを生成するツールセットです。

`SlideshowGenerator` は写真folderとPrefabからworld-space slideshowを生成します。`ExhibitionPackGenerator` は明示的なexhibition manifestを監査し、受理された写真からtexture・material・prefabを生成してreportを残します。

主な実装:

- `SlideshowGenerator.cs` — slideshow / exhibition pack生成UI
- `SlideshowController.cs` — slideshow runtime control
- `ExhibitionManifestPolicy.cs` — manifestと写真入力の監査
- `ExhibitionPackGenerator.cs` — texture / material / prefab / report生成
- `Photo Frame.prefab` / `Slideshow.prefab` — Unity Prefab

展示pack生成はmanifestで指定されたproject assetを対象にします。VRChat runtimeで利用者PCの写真folderを列挙する仕組みではありません。

## 写真展で使う

- [入力manifestの例](examples/exhibition.sample.json) — sample画像そのものはrepositoryに含めません。
- [写真展制作の提供範囲・有償PoC](docs/business/vr-photo-exhibition-pack.md) — 必要素材、生成物、権利確認、人間による確認、導入相談の入口をまとめています。
