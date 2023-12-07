/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiConnection
	{
		private static readonly byte[]                          _sig;
		private static readonly ConcurrentBag<FachpiConnection> _conns;
		private        readonly FachpiNode                      _owner;
		private        readonly Task<TcpClient>                 _task;
		private        readonly ConcurrentQueue<string>         _msgs;

		static FachpiConnection()
		{
			_sig = [
				0x46, 0x41, 0x43, 0x48, 0x50, 0x49, 0x00, 0xFF,
				0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF
			];
			_conns = [];
		}

		public FachpiConnection(FachpiNode owner, Task<TcpClient> task)
		{
			ArgumentNullException.ThrowIfNull(owner);
			ArgumentNullException.ThrowIfNull(task);

			_owner = owner;
			_task  = task;
			_msgs  = new();
			_conns.Add(this);
		}

		public void OnConnected()
		{
			_owner?.OnConnected();

			if (!_task.IsCompleted) {
				_task.Wait();
			}

			using (var flow = this.CreateCommunicationFlow(_task.Result)) {
				flow.Run();
			}
		}

		public virtual FachpiCommunicationFlow CreateCommunicationFlow(TcpClient tc)
			=> new(_owner, tc, _sig);

		public static IEnumerable<ConcurrentQueue<string>> GetMessageQueues()
			=> _conns.Select(c => c._msgs);
	}
}
