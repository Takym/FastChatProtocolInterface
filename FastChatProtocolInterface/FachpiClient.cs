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
using System.Threading;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiClient : FachpiNode
	{
		public void Connect(string hostName, int port)
		{
			var conn = new FachpiConnection(
				this,
				Task.FromResult(new TcpClient(hostName, port))
			);

			conn.OnConnected();
		}

		public override void OnConnected()
			=> Console.WriteLine("クライアントとして動作しています。");

		protected override void RunSenderProcessCore(BinaryWriter writer, ConcurrentQueue<string> messages, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				if (Console.ReadLine() is not null and string msg) {
					writer.Write(msg);
				}
			}
		}

		protected override void RunReceiverProcessCore(BinaryReader reader, NetworkStream ns, string remoteName, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				ns.WaitForDataAvailable();
				Console.WriteLine("{0}: {1}", remoteName, reader.ReadString());
			}
		}
	}
}
