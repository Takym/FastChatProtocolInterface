﻿/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Text;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public record SifoscObject
	{
		public virtual SifoscObject Plus    ()                    => this;
		public virtual SifoscObject Minus   ()                    => SifoscNull.Instance;
		public virtual SifoscObject Add     (SifoscObject? other) => SifoscNull.Instance;
		public virtual SifoscObject Subtract(SifoscObject? other) => SifoscNull.Instance;
	}

	public sealed record SifoscArray : SifoscObject
	{
		public ReadOnlyMemory<SifoscObject?> Values { get; init; }

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

		public override SifoscObject Add(SifoscObject? other)
		{
			if (other is SifoscArray otherArray) {
				int otherLen = otherArray.Values.Length;
				if (otherLen == 0) {
					return this;
				}

				int thisLen  = this.Values.Length;
				var newArray = new SifoscObject?[thisLen + otherLen];

				this      .Values.CopyTo(newArray);
				otherArray.Values.CopyTo(newArray.AsMemory()[thisLen..]);

				return this with { Values = newArray };
			}

			return SifoscNull.Instance;
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
		public int Value { get; init; }

		public override SifoscObject Minus()
			=> this with { Value = -this.Value };

		public override SifoscObject Add(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value + otherInt.Value };
			}

			return SifoscNull.Instance;
		}

		public override SifoscObject Subtract(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value - otherInt.Value };
			}

			return SifoscNull.Instance;
		}
	}
}
