/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net;

namespace FastChatProtocolInterface
{
	public static class EndPointExtensions
	{
		public static void Dump(this EndPoint? ep)
		{
			if (ep is null) {
				Console.WriteLine("エンドポイント情報を取得できませんでした。");
			} else {
				Console.WriteLine("アドレスの種類    ：{0}", ep.AddressFamily);
				Console.WriteLine("エンドポイントの型：{0}", ep.GetType().AssemblyQualifiedName);
				switch (ep) {
				case IPEndPoint ipep:
					Console.WriteLine("IP アドレス       ：{0}", ipep.Address.ToString());
					Console.WriteLine("ポート番号        ：{0}", ipep.Port);
					break;
				case DnsEndPoint dnsep:
					Console.WriteLine("ドメイン名        ：{0}", dnsep.Host);
					Console.WriteLine("ポート番号        ：{0}", dnsep.Port);
					break;
				default:
					Console.WriteLine("（不明なエンドポイントが指定されました。）");
					break;
				}
			}
			Console.WriteLine();
		}
	}
}
