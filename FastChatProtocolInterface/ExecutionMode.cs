/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using FastChatProtocolInterface.SimpleFormulaScript;

namespace FastChatProtocolInterface
{
	public abstract class ExecutionMode
	{
		private static readonly ConcurrentDictionary<string, ExecutionMode?> _modes;

		public static ExecutionMode Server => ServerImpl._inst_fachpi;
		public static ExecutionMode Client => ClientImpl._inst;

		static ExecutionMode()
		{
			_modes = new();
			_modes.TryAdd("server", ServerImpl._inst_fachpi);
			_modes.TryAdd("fachpi", ServerImpl._inst_fachpi);
			_modes.TryAdd("sifosc", ServerImpl._inst_sifosc);
			_modes.TryAdd("client", ClientImpl._inst);
			_modes.TryAdd("repl",   ReplImpl  ._inst);
		}

		public static ExecutionMode? GetMode(string name)
		{
			if (_modes.TryGetValue(name, out var result)) {
				return result;
			} else {
				return null;
			}
		}

		public abstract void Run(in CommandLineArguments args);

		private sealed class ServerImpl : ExecutionMode
		{
			internal static readonly ServerImpl _inst_fachpi = new((ip, port, name) => new FachpiServer(ip, port) { Name = name });
			internal static readonly ServerImpl _inst_sifosc = new((ip, port, name) => new SifoscServer(ip, port) { Name = name });

			private readonly Factory _factory;

			private ServerImpl(Factory factory)
			{
				_factory = factory;
			}

			public override void Run(in CommandLineArguments args)
			{
				using var server = _factory(IPAddress.Any, args.Port, args.UserName);

				server.Start();
				while (true) {
					Thread.Yield();
				}
			}

			private delegate FachpiServer Factory(IPAddress ip, int port, string name);
		}

		private sealed class ClientImpl : ExecutionMode
		{
			internal static readonly ClientImpl _inst = new();

			private ClientImpl() { }

			public override void Run(in CommandLineArguments args)
			{
				var client = new FachpiClient() {
					Name = args.UserName
				};

				client.Connect(args.HostName, args.Port);
			}
		}

		private sealed class ReplImpl : ExecutionMode
		{
			internal static readonly ReplImpl _inst = new();

			private ReplImpl() { }

			public override void Run(in CommandLineArguments args)
			{
				Console.WriteLine("SIFOSC を実行できます。");

				while (true) {
					Console.Write("> ");

					string? input = Console.ReadLine();
					if (!string.IsNullOrEmpty(input)) {
						string output = SifoscServer.RunScriptLine(input);

						Console.WriteLine(output);
						Console.WriteLine();
					}
				}
			}
		}
	}
}
