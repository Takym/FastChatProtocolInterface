/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Net.Sockets;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiClient : FachpiNode
	{
		public FachpiClient() { }

		public void Connect(string hostName, int port)
		{
			var conn = new FachpiConnection(
				this,
				Task.FromResult(new TcpClient(hostName, port))
			);

			conn.OnConnected();
		}
	}
}
