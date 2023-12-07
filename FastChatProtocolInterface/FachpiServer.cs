/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace FastChatProtocolInterface
{
	public class FachpiServer(IPAddress ipAddr, int port) : FachpiNode
	{
		private readonly TcpListener _listener = new(ipAddr, port);

		public void Start()
		{
			_listener.Start();

			Console.WriteLine("サーバーの接続情報");
			_listener.LocalEndpoint.Dump();

			Console.WriteLine("サーバーを開きました。");
			Console.WriteLine();

			this.Run();
		}

		public void Stop() => _listener.Stop();

		public override void OnConnected()
		{
			this.Run();
			Console.WriteLine("サーバーとして動作しています。");
		}

		private void Run()
		{
			var task = _listener.AcceptTcpClientAsync();
			var conn = new FachpiConnection(this, task);
			task.GetAwaiter().OnCompleted(conn.OnConnected);
		}

		protected override void RunSenderProcessCore(BinaryWriter writer, ConcurrentQueue<string> messages, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				if (messages.TryDequeue(out string? msg) && msg is not null) {
					writer.Write(msg);
				}
			}
		}

		protected override void RunReceiverProcessCore(BinaryReader reader, NetworkStream ns, string remoteName, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				ns.WaitForDataAvailable();
				string msg = string.Format(
					"[{0:yyyy/MM/dd HH\\:mm\\:ss.fffffff}]<{1}>{2}",
					DateTime.Now,
					remoteName,
					reader.ReadString()
				);
				Console.WriteLine(msg);
				foreach (var item in FachpiConnection.GetMessageQueues()) {
					item.Enqueue(msg);
				}
			}
		}
	}
}
