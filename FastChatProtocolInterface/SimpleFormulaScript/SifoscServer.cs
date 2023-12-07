/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public class SifoscServer(IPAddress ipAddr, int port) : FachpiServer(ipAddr, port)
	{
		public override void OnConnected()
		{
			base.OnConnected();

			Console.WriteLine("受信した文字列を SIFOSC として解析します。");
			Console.WriteLine("この機能には未実装の部分が含まれています。");
		}

		protected override string ReceiveMessageCore(FachpiCommunicationFlow flow)
		{
			var sc = new SourceCode(flow.Reader.ReadString());
			if (sc.TryParseValue(out var result)) {
				return result.ToString();
			} else {
				return "（有効な SIFOSC ではありませんでした。）" + sc.Text;
			}
		}
	}
}
