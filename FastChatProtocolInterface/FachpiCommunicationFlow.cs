using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiCommunicationFlow : IDisposable
	{
		private          bool                 _disposed;
		private readonly FachpiNode           _node;
		private readonly TcpClient            _tc;
		private readonly NetworkStream        _ns;
		private readonly BinaryReader         _br;
		private readonly BinaryWriter         _bw;
		private readonly ReadOnlyMemory<byte> _sig;
		private          bool                 _validated;
		private          string?              _local_name;
		private          string?              _remote_name;

		public bool IsDisposed => _disposed;

		public FachpiCommunicationFlow(FachpiNode node, TcpClient tc, ReadOnlyMemory<byte> sig)
		{
			ArgumentNullException.ThrowIfNull(node);
			ArgumentNullException.ThrowIfNull(tc);

			_disposed    = false;
			_node        = node;
			_tc          = tc;
			_ns          = tc.GetStream();
			_br          = new(_ns);
			_bw          = new(_ns);
			_sig         = sig;
			_validated   = false;
			_local_name  = null;
			_remote_name = null;

			Console.WriteLine("ローカルの接続情報");
			_ns.Socket.LocalEndPoint.Dump();
			Console.WriteLine("リモートの接続情報");
			_ns.Socket.RemoteEndPoint.Dump();
		}

		~FachpiCommunicationFlow()
		{
			this.Dispose(false);
		}

		public void Run()
		{
			this.ValidateNode();

			if (!_validated) {
				return;
			}

			this.ExchangeNodeName();

			Task.WaitAll(
				Task.Run(this.RunSender),
				Task.Run(this.RunReceiver)
			);
		}

		private void ValidateNode()
		{
			if (_validated) {
				return;
			}

			_ns.SendData(_sig.Span);
			if (_ns.ReceiveData().SequenceEqual(_sig.Span)) {
				_validated = true;
			} else {
				Console.WriteLine("通信先が不正な反応を示しました。");
			}
		}

		private void ExchangeNodeName()
		{
			if (_local_name is not null && _remote_name is not null) {
				return;
			}

			_local_name = _node.Name ?? string.Empty;
			_ns.SendText(_local_name);

			_remote_name = _ns.ReceiveText();

			Console.WriteLine("ローカルの名前：{0}", _local_name);
			Console.WriteLine("リモートの名前：{0}", _remote_name);
			Console.WriteLine();
		}

		private void RunSender()
			=> _node.RunSenderProcess(_bw, _msgs);

		private void RunReceiver()
			=> _node.RunReceiverProcess(_br, _ns, _remote_name);

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
				_bw.Dispose();
				_br.Dispose();
				_ns.Dispose();
				_tc.Dispose();
			}

			_disposed = true;
		}
	}
}
