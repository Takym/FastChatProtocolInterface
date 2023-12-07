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
		[STAThread()]
		private static int Main(string[] args)
		{
			try {
				var parsedArgs = CommandLineArguments.ParseArgs(args);
				parsedArgs.PrintHeader();
				parsedArgs.ExecutionMode?.Run(parsedArgs);
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
