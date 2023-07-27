/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net;
using System.Net.Sockets;

namespace FastChatProtocolInterface
{
	public class FachpiServer : FachpiNode
	{
		private readonly TcpListener _listener;

		public FachpiServer(IPAddress ipAddr, int port)
		{
			_listener = new(ipAddr, port);
		}

		public void Start()
		{
			_listener.Start();

			Console.WriteLine("サーバーの接続情報");
			_listener.LocalEndpoint.Dump();

			Console.WriteLine("サーバーを開きました。");
			Console.WriteLine();

			this.Run();
		}

		internal void Run()
		{
			var task = _listener.AcceptTcpClientAsync();
			var conn = new FachpiConnection(this, task);
			task.GetAwaiter().OnCompleted(conn.OnConnected);
		}

		public void Stop()
		{
			_listener.Stop();
		}
	}
}
