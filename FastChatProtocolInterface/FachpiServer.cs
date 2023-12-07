/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using MessageQueues = System.Collections.Concurrent.ConcurrentDictionary<
	FastChatProtocolInterface.FachpiCommunicationFlow,
	System.Collections.Concurrent.ConcurrentQueue<string>
>;

namespace FastChatProtocolInterface
{
	public class FachpiServer(IPAddress ipAddr, int port) : FachpiNode, IDisposable
	{
		private readonly TcpListener   _listener = new(ipAddr, port);
		private readonly MessageQueues _mq       = new();
		private          bool          _disposed = false;

		public bool IsDisposed => _disposed;

		~FachpiServer()
		{
			this.Dispose(false);
		}

		public void Start()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			_listener.Start();

			Console.WriteLine("サーバーの接続情報");
			_listener.LocalEndpoint.Dump();

			Console.WriteLine("サーバーを開きました。");
			Console.WriteLine();

			this.Run();
		}

		public void Stop() => _listener.Stop();

		private void Run()
		{
			var task = _listener.AcceptTcpClientAsync();
			var conn = this.CreateConnection(task);
			task.GetAwaiter().OnCompleted(conn.OnConnected);
		}

		protected virtual FachpiConnection CreateConnection(Task<TcpClient> acceptTcpClientTask)
			=> new(this, acceptTcpClientTask);

		public override void OnConnected()
		{
			ObjectDisposedException.ThrowIf(_disposed, this);

			this.Run();
			Console.WriteLine("サーバーとして動作しています。");
		}

		protected override void OnStartFlowCore(FachpiCommunicationFlow flow)
		{
			ObjectDisposedException.ThrowIf(_disposed, this);
			_mq.AddOrUpdate(flow, k => new(), (k, v) => v);
		}

		protected sealed override void RunSenderProcessCore(FachpiCommunicationFlow flow, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				ObjectDisposedException.ThrowIf(_disposed, this);

				if (_mq .TryGetValue(flow, out var     msgs) &&
					msgs.TryDequeue (      out string? msg ) &&
					msg is not null) {
					this.SendMessageCore(flow, msg);
				}
			}
		}

		protected virtual void SendMessageCore(FachpiCommunicationFlow flow, string message)
			=> flow.Writer.Write(message);

		protected sealed override void RunReceiverProcessCore(FachpiCommunicationFlow flow, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested) {
				ObjectDisposedException.ThrowIf(_disposed, this);

				flow.Stream.WaitForDataAvailable();
				string msg = string.Format(
					"[{0:yyyy/MM/dd HH\\:mm\\:ss.fffffff}]<{1}>{2}",
					DateTime.Now,
					flow.RemoteName,
					this.ReceiveMessageCore(flow)
				);
				Console.WriteLine(msg);
				foreach (var pair in _mq) {
					pair.Value.Enqueue(msg);
				}
			}
		}

		protected virtual string ReceiveMessageCore(FachpiCommunicationFlow flow)
			=> flow.Reader.ReadString();

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed) {
				return;
			}

			if (disposing) {
				_listener.Dispose();
			}

			foreach (var pair in _mq) {
				pair.Value.Clear();
			}
			_mq.Clear();

			_disposed = true;
		}
	}
}
