/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Threading;

namespace FastChatProtocolInterface
{
	public abstract class FachpiNode
	{
		private readonly string? _name;

		public string Name
		{
			get  => _name         ?? string.Empty;
			init => _name = value ?? string.Empty;
		}

		public abstract void OnConnected();

		public void OnStartFlow(FachpiCommunicationFlow flow)
		{
			ArgumentNullException.ThrowIfNull(flow);
			this.OnStartFlowCore(flow);
		}

		public void RunSenderProcess(FachpiCommunicationFlow flow, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(flow);
			this.RunSenderProcessCore(flow, cancellationToken);
		}

		public void RunReceiverProcess(FachpiCommunicationFlow flow, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(flow);
			this.RunReceiverProcessCore(flow, cancellationToken);
		}

		protected virtual  void OnStartFlowCore       (FachpiCommunicationFlow flow) { }
		protected abstract void RunSenderProcessCore  (FachpiCommunicationFlow flow, CancellationToken cancellationToken);
		protected abstract void RunReceiverProcessCore(FachpiCommunicationFlow flow, CancellationToken cancellationToken);
	}
}
