# VRPhotoJourney

VRPhotoJourney は、VRChat 内で没入感のあるフォトギャラリー体験を提供するツールです。このツールを使用すると、指定したフォルダ内の画像を自動的に読み込み、インタラクティブな VR フォトギャラリーを生成できます。

ユーザーは、VR 空間内を歩き回りながら、様々な画像を鑑賞することができます。画像の拡大表示、サムネイルの一覧表示、次の画像への移動など、直感的な操作が可能です。

VRPhotoJourney は、Unity エディタ上で動作し、簡単な設定でフォトギャラリーを作成できます。カスタマイズ可能なオプションも用意されているため、ユーザーは自分のニーズに合わせてギャラリーをパーソナライズできます。

VRChat 内で没入感のあるフォトギャラリー体験を提供したい方におすすめのツールです。

## 必要条件

- Unity
- VRChat SDK3
- VRChat Udon

## インストール

1. このリポジトリをクローンするか、ZIP ファイルをダウンロードして展開します。
2. Unity プロジェクトを開き、`Assets` フォルダ内に `Editor` フォルダと `Scripts` フォルダを配置します。
3. `Assets/Editor/SlideshowGenerator.cs` と `Assets/Scripts/SlideshowController.cs` ファイルが正しく配置されていることを確認します。

## 使用方法

1. Unity エディタのメニューバーから `Tools` > `Slideshow Generator` を選択し、Slideshow Generator ウィンドウを開きます。
2. `Photo Folder Path` フィールドに、スライドショーに使用する画像が格納されているフォルダのパスを入力します。
3. `Slideshow Prefab` フィールドに、スライドショーのベースとなるプレハブをアサインします。
4. `Photo Frame Prefab` フィールドに、個々の画像を表示するフォトフレームのプレハブをアサインします。
5. `Generate Slideshow` ボタンをクリックすると、指定されたフォルダ内の画像を使用してスライドショーが生成されます。

## スライドショーの操作

- `Enlarge Button`：現在表示されている画像を拡大表示します。
- `Overview Button`：すべての画像のサムネイルを一覧表示します。
- `Next Button`：次の画像を表示します。
- `Prev Button`：前の画像を表示します。
- `Folder Select Button`：新しいフォルダを選択し、スライドショーを更新します。

## カスタマイズ

`SlideshowController.cs` スクリプトを編集することで、スライドショーの動作をカスタマイズできます。以下は、カスタマイズ可能な主な機能です。

- `EnlargeCurrentSlideshow()`：現在の画像を拡大表示する処理を実装します。
- `ShowOverview()`：すべての画像のサムネイルを一覧表示する処理を実装します。
- `ShowNextSlideshow()`：次の画像を表示する処理を実装します。
- `ShowPreviousSlideshow()`：前の画像を表示する処理を実装します。
- `SetFolderPath()`：新しいフォルダを選択し、スライドショーを更新する処理を実装します。



---


## VRChat スライドショージェネレーターの期待動作

1. Unityエディタ上で`Tools/Slideshow Generator`を選択すると、`Slideshow Generator`ウィンドウが表示される。

2. `Slideshow Generator`ウィンドウには以下の要素が表示される：
   - `Photo Folder Path`：スライドショーに使用する画像のフォルダパスを入力するテキストフィールド。
   - `Slideshow Prefab`：スライドショーのプレハブをアサインするオブジェクトフィールド。
   - `Photo Frame Prefab`：フォトフレームのプレハブをアサインするオブジェクトフィールド。
   - `Generate Slideshow`：スライドショーを生成するボタン。

3. `Generate Slideshow`ボタンをクリックすると、以下の処理が実行される：
   - `Slideshow Parent`オブジェクトが生成される。
   - `Slideshow Parent`オブジェクトに`SlideshowController`コンポーネントが追加される。
   - `Slideshow Canvas`オブジェクトが生成され、`Slideshow Parent`の子オブジェクトになる。
   - `Folder Input Field`、`Enlarge Button`、`Overview Button`、`Next Button`、`Prev Button`、`Folder Select Button`のUIオブジェクトが生成され、`Slideshow Canvas`の子オブジェクトになる。
   - 生成されたUIオブジェクトが`SlideshowController`コンポーネントの対応するフィールドに割り当てられる。

4. `SlideshowController`コンポーネントの`Start()`メソッドが呼び出され、以下の処理が実行される：
   - 各ボタンのイベントリスナーが設定される。
   - `GenerateSlideshow()`メソッドが呼び出され、初期スライドショーが生成される。

5. `GenerateSlideshow()`メソッドでは以下の処理が実行される：
   - 既存のフォトフレームがすべて削除される。
   - 指定されたフォルダパスから画像ファイルのパスを取得する。
   - 取得した画像ファイルごとに、`Photo Frame Prefab`を複製してフォトフレームを生成する。
   - 生成されたフォトフレームが`Slideshow Parent`の子オブジェクトになる。
   - 最初のフォトフレームが表示される。

6. ボタンが押されたときの動作：
   - `Enlarge Button`：現在表示されているフォトフレームを拡大表示する。
   - `Overview Button`：すべてのフォトフレームのサムネイルを一覧表示する。
   - `Next Button`：次のフォトフレームを表示する。
   - `Prev Button`：前のフォトフレームを表示する。
   - `Folder Select Button`：新しいフォルダパスを設定し、スライドショーを更新する。

