/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Diagnostics;
using System.Net;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public class SifoscServer(IPAddress ipAddr, int port) : FachpiServer(ipAddr, port)
	{
		static SifoscServer()
		{
			bool succeeded = new SourceCode("[null,allobj,true,false]").TryParse(out _);

			Debug.Assert(succeeded);
		}

		public override void OnConnected()
		{
			base.OnConnected();

			Console.WriteLine("受信した文字列を SIFOSC として解析します。");
			Console.WriteLine("この機能には未実装の部分が含まれています。");
			Console.WriteLine();
		}

		protected override string ReceiveMessageCore(FachpiCommunicationFlow flow)
			=> RunScriptLine(flow.Reader.ReadString());

		public static string RunScriptLine(string s)
		{
			var sc = new SourceCode(s);
			if (sc.TryParse(out var result)) {
				return result.ToString();
			}

			return "（有効な SIFOSC ではありませんでした。）" + sc.Text;
		}
	}
}
