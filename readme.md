# VRPhotoJourney

[![Test](https://github.com/KAFKA2306/VRPhotoJourney/actions/workflows/test.yml/badge.svg)](https://github.com/KAFKA2306/VRPhotoJourney/actions/workflows/test.yml)

Unity Editor上で、指定したローカルフォルダの画像からスライドショー用オブジェクトを生成する試作です。

## 現在の保証範囲

- 対応画像: `.jpg` / `.jpeg` / `.png`
- 対象フォルダ: Unity Editorまたは通常のデスクトップ実行環境から読めるローカルフォルダ
- 存在しないフォルダ、読取不能フォルダ、空フォルダでは未処理例外を出さず、空状態へ移行
- 画像0件時は次・前・拡大・一覧ボタンを無効化
- 画像1件以上では次・前移動を安全に循環

`VRCUrlInputField` はURL入力用であり、ローカルファイルシステムのフォルダ選択には使用しません。公開済みVRChatワールドのUdon実行環境から、利用者PC上の任意フォルダを列挙する機能は本リポジトリの保証範囲外です。VRChat上で画像を配信する場合は、許可されたHTTPS URLとVRChat SDKの対応コンポーネントを使う別経路が必要です。

## 必要条件

- Unity
- Unity UI
- Editor生成機能を使う場合はUnity Editor

## 配置

- `SlideshowGenerator.cs`: `Assets/Editor/`
- `SlideshowController.cs`: `Assets/Scripts/`
- `SlideshowPathPolicy.cs`: `Assets/Scripts/`

## 使用方法

1. Unityの `Tools > Slideshow Generator` を開く。
2. `Photo Folder Path` に読取可能なローカルフォルダを指定する。
3. Slideshow PrefabとPhoto Frame Prefabを割り当てる。
4. `Generate Slideshow` を実行する。

無効なパスまたは対応画像0件の場合、既存表示はクリアされ、状態メッセージが表示されます。フォルダ内の非画像ファイルは無視されます。

## 検証

純粋なパス列挙とインデックス計算はUnityから分離されています。

```bash
dotnet test tests/SlideshowPathPolicy.Tests/SlideshowPathPolicy.Tests.csproj
```

CIでは、対応拡張子だけの決定論的列挙、存在しないパスの拒否、0件・1件・複数件のインデックス境界を外部サービスなしで検証します。

## 未実装

- フレームへの実テクスチャ割り当て
- 拡大表示と一覧表示
- VRChat runtimeでのリモート画像配信
- 実ワールド上のUdon互換性検証
