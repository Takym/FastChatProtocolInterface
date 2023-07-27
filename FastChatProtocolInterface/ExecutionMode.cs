/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Collections.Concurrent;
using System.Net;
using System.Threading;

namespace FastChatProtocolInterface
{
	public abstract class ExecutionMode
	{
		private static readonly ConcurrentDictionary<string, ExecutionMode?> _modes;

		public static ExecutionMode Server => ServerImpl._inst;
		public static ExecutionMode Client => ClientImpl._inst;

		static ExecutionMode()
		{
			_modes = new();
			_modes.TryAdd("server", ServerImpl._inst);
			_modes.TryAdd("client", ClientImpl._inst);
		}

		public static ExecutionMode? GetMode(string name)
		{
			if (_modes.TryGetValue(name, out var result)) {
				return result;
			} else {
				return null;
			}
		}

		public abstract void Run(in CommandLineArgument args);

		private sealed class ServerImpl : ExecutionMode
		{
			internal static readonly ServerImpl _inst = new();

			private ServerImpl() { }

			public override void Run(in CommandLineArgument args)
			{
				var server = new FachpiServer(IPAddress.Any, args.Port) {
					Name = args.UserName
				};

				server.Start();
				while (true) {
					Thread.Yield();
				}
			}
		}

		private sealed class ClientImpl : ExecutionMode
		{
			internal static readonly ClientImpl _inst = new();

			private ClientImpl() { }

			public override void Run(in CommandLineArgument args)
			{
				var client = new FachpiClient() {
					Name = args.UserName
				};

				client.Connect(args.HostName, args.Port);
			}
		}
	}
}
