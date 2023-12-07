/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Text;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public record SifoscObject { }

	public sealed record SifoscArray : SifoscObject
	{
		public ReadOnlyMemory<SifoscObject?> Values { get; set; }

		protected override bool PrintMembers(StringBuilder builder)
		{
			// 参考：https://qiita.com/muniel/items/fd843abc55a5626e5c45

			var s = this.Values.Span;
			for (int i = 0; i < s.Length; ++i) {
				if (i != 0) {
					builder.Append(", ");
				}
				builder.Append(s[i]);
			}
			return s.Length > 0;
		}
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
