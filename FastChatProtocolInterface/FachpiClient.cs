/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiClient : FachpiNode
	{
		public virtual void Connect(string hostName, int port)
		{
			var conn = new FachpiConnection(
				this,
				Task.FromResult(new TcpClient(hostName, port))
			);

			conn.OnConnected();
		}

		public override void OnConnected()
			=> Console.WriteLine("クライアントとして動作しています。");

		protected override void RunSenderProcessCore(FachpiCommunicationFlow flow, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				if (Console.ReadLine() is not null and string msg) {
					flow.Writer.Write(msg);
				}
			}
		}

		protected override void RunReceiverProcessCore(FachpiCommunicationFlow flow, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				flow.Stream.WaitForDataAvailable();
				Console.WriteLine("{0}: {1}", flow.RemoteName, flow.Reader.ReadString());
			}
		}
	}
}
