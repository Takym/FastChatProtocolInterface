/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public record SifoscObject { }

	public sealed record SifoscArray : SifoscObject
	{
		public SifoscObject?[]? Values { get; set; }
	}

	public sealed record SifoscNull : SifoscObject
	{
		private static readonly SifoscNull _inst = new();

		public static SifoscNull Instance => _inst;

		private SifoscNull() { }
	}

	public sealed record SifoscInteger : SifoscObject
	{
		public int Value { get; set; }
	}
}
