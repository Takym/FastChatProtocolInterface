/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FastChatProtocolInterface
{
	public static class NetworkStreamExtensions
	{
		private static readonly Encoding _enc;

		static NetworkStreamExtensions()
		{
			_enc = new UTF8Encoding(false, false);
		}

		public static void WaitForDataAvailable(this NetworkStream ns)
		{
			while (!ns.DataAvailable) {
				Thread.Yield();
			}
		}

		public static void SendText(this NetworkStream ns, string text)
			=> SendData(ns, _enc.GetBytes(text));

		public static string ReceiveText(this NetworkStream ns)
			=> _enc.GetString(ReceiveData(ns));

		public static void SendData(this NetworkStream ns, ReadOnlySpan<byte> data)
		{
			Span<byte> lenBuf = stackalloc byte[4];
			if (BitConverter.TryWriteBytes(lenBuf, data.Length)) {
				ns.Write(lenBuf);
				ns.Write(data);
			} else {
				Console.WriteLine("データ量を変換できなかった為、データを送信できませんでした。");
			}
		}

		public static ReadOnlySpan<byte> ReceiveData(this NetworkStream ns)
		{
			WaitForDataAvailable(ns);
			if (ns.DataAvailable) {
				Span<byte> lenBuf = stackalloc byte[4];
				ns.Read(lenBuf);

				int len = BitConverter.ToInt32(lenBuf);
				if (len > 0) {
					Span<byte> buf = new byte[len];
					for (int i = 0; i < buf.Length; ++i) {
						WaitForDataAvailable(ns);
						if (ns.DataAvailable) {
							int data = ns.ReadByte();
							if (data is >= byte.MinValue and <= byte.MaxValue) {
								buf[i] = unchecked((byte)(data));
							} else {
								Console.WriteLine("無効なバイトデータを読み取った為、受信できませんでした。");
								return default;
							}
						} else {
							Console.WriteLine("データの送信が中断された為、受信できませんでした。");
							return default;
						}
					}
					return buf;
				} else if (len == 0) {
					Console.WriteLine("データが送信されなかった為、受信できませんでした。");
					return default;
				} else {
					Console.WriteLine("データ量に負数 {0} が指定された為、受信できませんでした。", len);
					return default;
				}
			} else {
				Console.WriteLine("有効なデータを受信できませんでした。");
				return default;
			}
		}
	}
}
