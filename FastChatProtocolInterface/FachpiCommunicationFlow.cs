/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FastChatProtocolInterface
{
	public class FachpiCommunicationFlow : IDisposable
	{
		private static readonly ReadOnlyMemory<byte> _sig;
		private        readonly FachpiConnection     _conn;
		private        readonly FachpiNode           _node;
		private        readonly TcpClient            _tc;
		private        readonly NetworkStream        _ns;
		private        readonly BinaryReader         _br;
		private        readonly BinaryWriter         _bw;
		private                 bool                 _disposed;
		private                 bool                 _validated;
		private                 string?              _local_name;
		private                 string?              _remote_name;

		public FachpiConnection Connection  => _conn;
		public FachpiNode       Node        => _node;
		public bool             IsDisposed  => _disposed;
		public bool             IsValidated => _validated;
		public string?          LocalName   => _local_name;
		public string?          RemoteName  => _remote_name;

		public NetworkStream Stream
		{
			get
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _ns;
			}
		}

		public BinaryReader Reader
		{
			get
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _br;
			}
		}

		public BinaryWriter Writer
		{
			get
			{
				ObjectDisposedException.ThrowIf(_disposed, this);
				return _bw;
			}
		}
		
		static FachpiCommunicationFlow()
		{
			_sig = new byte[] {
				0x46, 0x41, 0x43, 0x48, 0x50, 0x49, 0x00, 0xFF,
				0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF
			};
		}

		public FachpiCommunicationFlow(FachpiConnection conn, TcpClient tc)
		{
			ArgumentNullException.ThrowIfNull(conn);
			ArgumentNullException.ThrowIfNull(tc);

			_conn        = conn;
			_node        = conn.Owner;
			_tc          = tc;
			_ns          = tc.GetStream();
			_br          = new(_ns);
			_bw          = new(_ns);
			_disposed    = false;
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
			_node.OnStartFlow(this);

			_validated = this.ValidateNode();
			if (!_validated) {
				Console.WriteLine("通信先が不正な反応を示しました。");
				return;
			}

			(_local_name, _remote_name) = this.ExchangeNodeName();
			Console.WriteLine("ローカルの名前：{0}", _local_name);
			Console.WriteLine("リモートの名前：{0}", _remote_name);
			Console.WriteLine();

			Task.WaitAll(
				Task.Run(this.RunSender),
				Task.Run(this.RunReceiver)
			);
		}

		protected virtual bool ValidateNode()
		{
			_ns.SendData(_sig.Span);
			return _ns.ReceiveData().SequenceEqual(_sig.Span);
		}

		protected virtual (string localName, string remoteName) ExchangeNodeName()
		{
			string ln = _node.Name ?? string.Empty;
			_ns.SendText(ln);

			string rn = _ns.ReceiveText();

			return (ln, rn);
		}

		protected virtual void RunSender()
			=> _node.RunSenderProcess(this);

		protected virtual void RunReceiver()
			=> _node.RunReceiverProcess(this);

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
