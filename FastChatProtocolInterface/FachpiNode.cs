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

		public void RunSenderProcess(BinaryWriter writer, ConcurrentQueue<string> messages, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(writer);
			ArgumentNullException.ThrowIfNull(messages);

			this.RunSenderProcessCore(writer, messages, cancellationToken);
		}

		public void RunReceiverProcess(BinaryReader reader, NetworkStream ns, string remoteName, CancellationToken cancellationToken = default)
		{
			ArgumentNullException.ThrowIfNull(reader);
			ArgumentNullException.ThrowIfNull(remoteName);

			this.RunReceiverProcessCore(reader, ns, remoteName, cancellationToken);

			/* if (reader.BaseStream is NetworkStream ns) {
				this.RunReceiverProcessCore(reader, ns, messageQueues, remoteName, cancellationToken);
			} else {
				throw new ArgumentException("The underlying stream of the reader must be a network stream.", nameof(reader));
			} //*/
		}

		protected abstract void RunSenderProcessCore(BinaryWriter writer, ConcurrentQueue<string> messages, CancellationToken cancellationToken);

		protected abstract void RunReceiverProcessCore(BinaryReader reader, NetworkStream ns, string remoteName, CancellationToken cancellationToken);
	}
}
