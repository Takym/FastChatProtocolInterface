# FACHPI: Fast Chat Protocol/Interface
# 高速会話プロトコル・インターフェース
Copyright (C) 2023 Takym.

## 概要
このツールは、単純な文字列のみの通信を行います。

## 使用方法

### ビルド方法
0. [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0)をインストールしてください。
1. このリポジトリをクローンしてください。
	* `git clone https://github.com/Takym/FastChatProtocolInterface.git`
2. 作業ディレクトリを `FastChatProtocolInterface` へ移動してください。
	* `cd FastChatProtocolInterface`
3. ビルドしてください。
	* `dotnet build`

### 実行方法
1. 「ビルド方法」と同等の方法でクローンされているものとします。
2. 作業ディレクトリを `FastChatProtocolInterface\bin\Debug\net7.0` へ移動してください。
	* `cd FastChatProtocolInterface\bin\Debug\net7.0`
	* ビルド設定を変更した場合、別のディレクトリに出力されている可能性があります。
3. 実行してください。
	* `fachpi -v -h`
	* `fachpi -m server [オプション...]`
	* `fachpi -m client -n <ホスト名> -p <ポート番号> [オプション...]`

### コマンド行引数
0. 最新の説明書を表示するにはこの方法を試してください。
1. 「ビルド方法」と同等の方法でクローンされているものとします。
2. 下記のコマンドを実行してください。
	* `dotnet run --project FastChatProtocolInterface -- -v -h`
3. 下記の様な説明書が表示される筈です。
	```
	FACHPI: Fast Chat Protocol/Interface
	Copyright (C) 2023 Takym.

	バージョン：0.0.0.0

	FACHPI コマンド行引数説明書
	===========================

	使用法> fachpi [オプション...]
	使用法> fachpi -m server [オプション...]
	使用法> fachpi -m client -n <ホスト名> -p <ポート番号> [オプション...]

	使用例> fachpi -m server -p 1024 -u "God" [オプション...]
	使用例> fachpi -m client -n 192.168.0.2 -p 1024 [オプション...]
	使用例> fachpi -m client -n 127.0.0.1 -p 48000 -u "Hoge" [オプション...]

	オプション一覧
	長い形式        短い形式  説明
	/Version        -v        バージョン情報を表示する。
	/Help           -h        この説明書を表示する。
	/NoLogo                   題名と著作権情報の表示を抑制する。
	/ExecutionMode  -m        実行モードを指定する。
	                          サーバーの場合は、server を指定する。
	                          クライアントの場合は、client を指定する。
	/HostName       -n        ホスト名を指定する。
	                          この値はクライアントの場合のみ使用される。
	/Port           -p        ポート番号を指定する。
	                          クライアントの場合は必ず指定しなければならない。
	/UserName       -u        利用者の表示名を指定する。

	注意：幾つかのオプションにはこの説明書に記載されていない別表記があります。
	```

## 利用規約
[MITライセンス](./LICENSE.md)に従うものとします。
