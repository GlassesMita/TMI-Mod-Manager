# <ruby title="You Know Too Much">Tou<rt>T</rt>hou<rt>H</rt> Mystia<rt>M</rt> Izakiya<rt>I</rt></ruby> Mod Manager
<font size=75%><del>TMIとも呼ばれるTHMIは、公式の名前</del></font>

![Windows](https://img.shields.io/badge/Windows-0078D6?style=for-the-badge&logo=windows&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![Unity](https://img.shields.io/badge/Unity-100000?style=for-the-badge&logo=unity&logoColor=white)
<!-- ![Git](https://img.shields.io/badge/GIT-E44C30?style=for-the-badge&logo=git&logoColor=white) -->
<!-- ![Windowsターミナル](https://img.shields.io/badge/windows%20terminal-4D4D4D?style=for-the-badge&logo=windows%20terminal&logoColor=white) -->
![Visual Studio](https://img.shields.io/badge/Visual_Studio-5C2D91?style=for-the-badge&logo=visual%20studio&logoColor=white)

[英語](README_en.md) | [簡体字中国語](README.md) | (日本語)(READEME_ja.md)<sub>\(現在のファイル\)</sub>

**このプロジェクトはGNU GPL 3ライセンスを使用しています**

## プロジェクト概要

TMI Mod Managerは、Night Sparrow CanteenのModを管理および整理するためのツールです。ゲーム内でModを簡単に追加、削除、管理するためのシンプルなインターフェースをユーザーに提供することを目的としています。
ご提案やアイデアがありましたら、QQグループ 470175141 にご参加ください。

<詳細>
<概要>⭐こちらをクリック⭐して⭐美しい⭐プロモーション資料をご覧ください⭐</概要>

！Bird Food MODに関する議論のための唯一の集いの場をご紹介します。
[470175141](https://qm.qq.com/q/ZjHPtumekw) [470175141](https://qm.qq.com/q/ZjHPtumekw) [470175141](https://qm.qq.com/q/ZjHPtumekw) (大切なことなので3回繰り返します)
プログラマー、アーティスト、ミュージシャン、プランナー（こんなことって本当に必要なの？）、あるいは学びやアイデア交換を求めている一般プレイヤー、あるいはBird Food MODの素晴らしさを知りたいだけの方、ぜひこのグループチャットにご参加ください！

![Chemical Lab](https://raw.githubusercontent.com/GlassesMita/TMI-Mod-Manager/main/Assets/Images/%E5%8C%96%E5%AD%A6%E5%AE%9E%E9%AA%8C%E5%AE%A4.jpg)

</details>

> [!重要]
> **免責事項**
>
> **注意**: このプロジェクトは非公式であり、元の開発チームとは一切関係がありません。ユーザーの皆様に便利なMOD管理ツールを提供することを目指しておりますが、このプロジェクトの使用には一定のリスクが伴う可能性があることをご承知おきください。使用前に関連ドキュメントをよくお読みいただき、自己責任でご使用ください。

> [!ヒント]
> このプロジェクトはMod Managerのデモプロジェクトとして機能します。

## 機能

> [!注意]
> 以下の機能は現在開発中であり、変更または削除される可能性があります。
>
> 正確性については、最新リリース版をご確認ください。

- [x] **Mod管理**: Modを簡単に追加、削除、更新できます。
- [x] **多言語サポート**: 中国語、英語、日本語、その他の言語をサポートしています。必要なファイルがない場合は、手動で翻訳できます。
- [x] **ウィンドウタイトルの変更**: ウィンドウタイトルを自動変更し、識別しやすくします。
- [x] **ショートカットコントロール**: 組み込みのキーボードショートカットを使用して、操作をすばやく実行できます。
- [x] **UIテキストの読み上げ**: テキストを右クリックすると、テキストの音声が読み上げられます。
- [ ] **ユーザーフレンドリーなインターフェース**: シンプルで直感的なユーザーインターフェースで、簡単に操作できます。
- [ ] **互換性チェック**: Mod間の互換性を自動チェックし、競合を回避します。
- [ ] **Xboxコントローラーのサポート**: Xboxコントローラーを使用して、ユーザーインターフェース全体を操作できます。
- [ ] その他の機能は現在検討中です...

## ファイル構造

- [**Assets**](Assets): プロジェクトのリソースファイルが格納されます。
- [**Packages**](Packages): プロジェクトの依存パッケージが格納されます。
- [**ProjectSettings**](ProjectSettings): プロジェクト設定ファイル

## INI 設定ファイルの構造
<font size=125%>`AppConfig.ini`:</font>
- ***\[Config\]***: 基本設定
- ***\[Localization\]***: ローカリゼーション設定
- ***\[Title\]***: 表示タイトル

## 環境要件

- **Unity バージョン**: 2021.3.28f1
- **ビルドタイプ**: Mono
- **アーキテクチャ**: x64

## ビルドと実行

1. リポジトリをローカルにクローンします。

```bash
git clone https://github.com/GlassesMita/TMI-Mod-Manager.git
```

2. Unity Hub を開き、プロジェクトを追加します。
- 「追加」ボタンをクリックし、クローンしたプロジェクトフォルダを選択します。
3. プロジェクトを開いて実行します。
- Unityエディターでプロジェクトを開き、「ファイル」→「ビルド設定」をクリックします。
- 「ビルド」の右側にある▼アイコンをクリックし、「クリーンビルド」を選択します。
- ビルド出力フォルダを選択します。*SteamLibrary/SteamApps/Common/Touhou Mystia Izakiya/Mod Manager* を推奨します。
- ビルドが完了したら、クローンしたリポジトリから**AppConfig.ini**ファイルをTMI Mod Manager.exeと同じディレクトリにコピーします。
- TMI Mod Manager.exeを実行します。

## プロジェクト例
![メインUI](QQ20250910-115819.png)
![設定](32d3cef1-df98-4743-bcc4-ed3d3df0e94c.png)
![バージョン情報](QQ20250910-115929.png)
