/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Runtime.CompilerServices;

#if !DEBUG
using System;
#endif

namespace FastChatProtocolInterface
{
	internal static class Program
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void Run(string[] args)
		{
			var parsedArgs = CommandLineArguments.ParseArgs(args);
			parsedArgs.PrintHeader();
			parsedArgs.ExecutionMode?.Run(parsedArgs);
		}

#if DEBUG
		private static void Main(string[] args) => Run(args);
#else
		[STAThread()]
		private static int Main(string[] args)
		{
			try {
				Run(args);
				return 0;
			} catch (Exception e) {
				Console.ForegroundColor = ConsoleColor.Red;
				Console.Error.WriteLine();
				Console.Error.WriteLine(e.ToString());
				Console.ResetColor();
				return e.HResult;
			}
		}
#endif
	}
}
