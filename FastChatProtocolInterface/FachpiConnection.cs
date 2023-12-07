/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiConnection
	{
		private readonly FachpiNode      _owner;
		private readonly Task<TcpClient> _task;

		public FachpiNode Owner => _owner;

		public FachpiConnection(FachpiNode owner, Task<TcpClient> acceptTcpClientTask)
		{
			ArgumentNullException.ThrowIfNull(owner);
			ArgumentNullException.ThrowIfNull(acceptTcpClientTask);

			_owner = owner;
			_task  = acceptTcpClientTask;
		}

		public void OnConnected()
		{
			_owner?.OnConnected();

			if (!_task.IsCompleted) {
				_task.Wait();
			}

			using (var flow = this.CreateCommunicationFlow(_task.Result)) {
				this.RunCommunicationFlow(flow);
			}
		}

		protected virtual FachpiCommunicationFlow CreateCommunicationFlow(TcpClient tc)
			=> new(this, tc);

		protected virtual void RunCommunicationFlow(FachpiCommunicationFlow flow)
			=> flow.Run();
	}
}
