/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;

namespace FastChatProtocolInterface
{
	internal static class Program
	{
		private static void Run(in CommandLineArgument args)
		{
			args.PrintHeader();
			args.ExecutionMode?.Run(args);
		}

		[STAThread()]
		private static int Main(string[] args)
		{
			try {
				Run(CommandLineArgument.ParseArgs(args));
				return 0;
			} catch (Exception e) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine();
				Console.Error.WriteLine(e.ToString());
				Console.ResetColor();
				return e.HResult;
			}
		}
	}
}
