/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiConnection
	{
		private static readonly byte[]                          _sig;
		private static readonly Encoding                        _enc;
		private static readonly ConcurrentBag<FachpiConnection> _conns;
		private        readonly FachpiNode?                     _owner;
		private        readonly Task<TcpClient>                 _task;
		private        readonly ConcurrentQueue<string>         _msgs;

		static FachpiConnection()
		{
			_sig = [
				0x46, 0x41, 0x43, 0x48, 0x50, 0x49, 0x00, 0xFF,
				0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF
			];
			_enc   = new UTF8Encoding(false, false);
			_conns = [];
		}

		public FachpiConnection(FachpiNode owner, Task<TcpClient> task)
		{
			_owner = owner;
			_task  = task;
			_msgs  = new();
			_conns.Add(this);
		}

		public void OnConnected()
		{
			if (_owner is FachpiServer server) {
				server.Run();
				Console.WriteLine("サーバーとして動作しています。");
			} else {
				Console.WriteLine("クライアントとして動作しています。");
			}

			if (!_task.IsCompleted) {
				_task.Wait();
			}

			using (var tc = _task.Result)
			using (var ns = tc.GetStream()) {
				Console.WriteLine("ローカルの接続情報");
				ns.Socket.LocalEndPoint.Dump();
				Console.WriteLine("リモートの接続情報");
				ns.Socket.RemoteEndPoint.Dump();

				this.RunCommunicationFlow(tc, ns);
			}
		}

		public virtual void RunCommunicationFlow(TcpClient tc, NetworkStream ns)
		{
			using (var br = new BinaryReader(ns))
			using (var bw = new BinaryWriter(ns)) {
				string localName = _owner?.Name ?? string.Empty;
				string remoteName;

				SendData(ns, _sig);
				if (!ReceiveData(ns).SequenceEqual(_sig)) {
					Console.WriteLine("通信先が不正な反応を示しました。");
					return;
				}

				SendText(ns, localName);
				remoteName = ReceiveText(ns);

				Console.WriteLine("ローカルの名前：{0}", localName);
				Console.WriteLine("リモートの名前：{0}", remoteName);
				Console.WriteLine();

				var sender = _owner is FachpiServer
					? Task.Run(() => {
						while (true) {
							if (_msgs.TryDequeue(out string? msg) && msg is not null) {
								bw.Write(msg);
							}
						}
					})
					: Task.Run(() => {
						while (true) {
							if (Console.ReadLine() is not null and string msg) {
								bw.Write(msg);
							}
						}
					});

				var receiver = _owner is FachpiServer
					? Task.Run(() => {
						while (true) {
							WaitForDataAvailable(ns);
							string msg = string.Format(
								"[{0:yyyy/MM/dd HH\\:mm\\:ss.fffffff}]<{1}>{2}",
								DateTime.Now,
								remoteName,
								br.ReadString()
							);
							Console.WriteLine(msg);
							foreach (var item in _conns) {
								item._msgs.Enqueue(msg);
							}
						}
					})
					: Task.Run(() => {
						while (true) {
							WaitForDataAvailable(ns);
							Console.WriteLine("{0}: {1}", remoteName, br.ReadString());
						}
					});

				Task.WaitAll(sender, receiver);
			}
		}

		protected static void WaitForDataAvailable(NetworkStream ns)
		{
			while (!ns.DataAvailable) {
				Thread.Yield();
			}
		}

		protected static void SendText(NetworkStream ns, string text)
			=> SendData(ns, _enc.GetBytes(text));

		protected static string ReceiveText(NetworkStream ns)
			=> _enc.GetString(ReceiveData(ns));

		protected static void SendData(NetworkStream ns, ReadOnlySpan<byte> data)
		{
			Span<byte> lenBuf = stackalloc byte[4];
			if (BitConverter.TryWriteBytes(lenBuf, data.Length)) {
				ns.Write(lenBuf);
				ns.Write(data);
			} else {
				Console.WriteLine("データ量を変換できなかった為、データを送信できませんでした。");
			}
		}

		protected static ReadOnlySpan<byte> ReceiveData(NetworkStream ns)
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
