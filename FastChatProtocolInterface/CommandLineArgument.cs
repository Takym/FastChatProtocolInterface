/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Reflection;
using System.Text;

namespace FastChatProtocolInterface
{
	public readonly record struct CommandLineArgument(
		Assembly       Assembly,
		bool           ShowVersion,
		bool           ShowHelp,
		bool           ShowLogo,
		ExecutionMode? ExecutionMode,
		string         HostName,
		int            Port,
		string         UserName,
		StringBuilder? Warnings
	)
	{
		public void PrintHeader()
		{
			if (this.ShowLogo) {
				Console.WriteLine("FACHPI: Fast Chat Protocol/Interface");
				Console.WriteLine(this.Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>()?.Copyright);
				Console.WriteLine();
			}

			if (this.ShowVersion) {
				var ver = this.Assembly.GetName().Version;
				if (ver is null) {
					Console.WriteLine("現在、バージョン番号はまだ制定されていません。");
				} else {
					Console.WriteLine("バージョン：{0}", ver);
				}
				Console.WriteLine();
			}

			if (this.ShowHelp) {
				PrintHelp();
			}

			string? w = this.Warnings?.ToString();
			if (!string.IsNullOrEmpty(w)) {
				Console.WriteLine("コマンド行引数に関する警告：");
				Console.WriteLine(w);
			}
		}

		public static CommandLineArgument ParseArgs(string[] args)
		{
			var result = new CommandLineArgument(
				Assembly:      typeof(CommandLineArgument).Assembly,
				ShowVersion:   false,
				ShowHelp:      false,
				ShowLogo:      true,
				ExecutionMode: null,
				HostName:      "localhost",
				Port:          0,
				UserName:      "名無し",
				Warnings:      new()
			);

			if (args is null) {
				result.Warnings!.AppendLine("コマンド行引数に null が指定されました。");
				return result;
			}

			for (int i = 0; i < args.Length; ++i) {
				string arg   = args[i].ToLower();
				int    iNext = i + 1;
				switch (arg) {
				case "/v" or "/ver" or "/version":
				case "-v" or "--ver" or "--version":
					result = result with { ShowVersion = true };
					break;
				case "/?" or "/h" or "/help" or "/man" or "/manual":
				case "-?" or "-h" or "--help" or "--man" or "--manual":
					result = result with { ShowHelp = true };
					break;
				case "/nologo" or "--nologo" or "--no-logo":
					result = result with { ShowLogo = false };
					break;
				case "/m" or "/mode" or "-m" or "--mode":
				case "/executionmode" or "--execution-mode":
					if (iNext < args.Length) {
						i = iNext;

						string modeName = args[i];
						result = result with { ExecutionMode = ExecutionMode.GetMode(modeName) };

						if (result.ExecutionMode is null) {
							result.Warnings!
								.AppendFormat("不明な実行モード「{0}」が指定されました。", modeName)
								.AppendLine();
						}
					} else {
						result.Warnings!.AppendLine("実行モードが指定されていません。");
					}
					break;
				case "/n" or "/hostname" or "-n" or "--host-name":
					if (iNext < args.Length) {
						i      = iNext;
						result = result with { HostName = args[i] };
					} else {
						result.Warnings!.AppendLine("ホスト名が指定されていません。");
					}
					break;
				case "/p" or "/port" or "-p" or "--port":
					if (iNext < args.Length) {
						i = iNext;
						if (int.TryParse(args[i], out int port)) {
							result = result with { Port = port };
						} else {
							result.Warnings!.AppendLine("ポート番号は数値で指定してください。");
						}
					} else {
						result.Warnings!.AppendLine("ポート番号が指定されていません。");
					}
					break;
				case "/u" or "/username" or "-u" or "--user-name":
					if (iNext < args.Length) {
						i      = iNext;
						result = result with { UserName = args[i] };
					} else {
						result.Warnings!.AppendLine("利用者の表示名が指定されていません。");
					}
					break;
				default:
					result.Warnings!
						.Append(i + 1)
						.Append(" 番目に不正な引数「")
						.Append(arg)
						.AppendLine("」が指定されました。");
					break;
				}
			}
			return result;
		}

		private static void PrintHelp()
		{
			Console.WriteLine("FACHPI コマンド行引数説明書");
			Console.WriteLine("===========================");
			Console.WriteLine();
			Console.WriteLine("使用法> fachpi [オプション...]");
			Console.WriteLine("使用法> fachpi -m server [オプション...]");
			Console.WriteLine("使用法> fachpi -m client -n <ホスト名> -p <ポート番号> [オプション...]");
			Console.WriteLine();
			Console.WriteLine("使用例> fachpi -m server -p 1024 -u \"God\" [オプション...]");
			Console.WriteLine("使用例> fachpi -m client -n 192.168.0.2 -p 1024 [オプション...]");
			Console.WriteLine("使用例> fachpi -m client -n 127.0.0.1 -p 48000 -u \"Hoge\" [オプション...]");
			Console.WriteLine();
			Console.WriteLine("オプション一覧");
			Console.WriteLine("長い形式        短い形式  説明");
			Console.WriteLine("/Version        -v        バージョン情報を表示する。");
			Console.WriteLine("/Help           -h        この説明書を表示する。");
			Console.WriteLine("/NoLogo                   題名と著作権情報の表示を抑制する。");
			Console.WriteLine("/ExecutionMode  -m        実行モードを指定する。");
			Console.WriteLine("                          サーバーの場合は、server を指定する。");
			Console.WriteLine("                          クライアントの場合は、client を指定する。");
			Console.WriteLine("/HostName       -n        ホスト名を指定する。");
			Console.WriteLine("                          この値はクライアントの場合のみ使用される。");
			Console.WriteLine("/Port           -p        ポート番号を指定する。");
			Console.WriteLine("                          クライアントの場合は必ず指定しなければならない。");
			Console.WriteLine("/UserName       -u        利用者の表示名を指定する。");
			Console.WriteLine();
			Console.WriteLine("注意：幾つかのオプションにはこの説明書に記載されていない別表記があります。");
			Console.WriteLine();
		}
	}
}
