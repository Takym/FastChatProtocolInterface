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
	public sealed record SifoscArray : SifoscObject
	{
		public override string Code
		{
			get
			{
				var sb   = new StringBuilder();
				var span = this.Values.Span;

				sb.Append('[');
				for (int i = 0; i < span.Length; ++i) {
					var obj = span[i];
					if (obj is null) {
						continue;
					}
					if (i != 0) {
						sb.Append(',');
					}
					sb.Append(obj.Code);
				}
				sb.Append(']');

				return sb.ToString();
			}
		}

		public ReadOnlyMemory<SifoscObject?> Values { get; init; }

		protected override bool PrintMembers(StringBuilder builder)
		{
			// 参考：https://qiita.com/muniel/items/fd843abc55a5626e5c45

			bool appendComma = base.PrintMembers(builder);

			var s = this.Values.Span;
			for (int i = 0; i < s.Length; ++i) {
				if (appendComma) {
					builder.Append(", ");
				}
				var obj = s[i];
				if (obj is SifoscViewForAllObjects view) {
					view.PrintMembersSimply(builder);
				} else {
					builder.Append(obj);
				}
				appendComma = true;
			}
			return appendComma;
		}

		public override SifoscObject? Add(SifoscObject? other)
		{
			if (other is SifoscArray otherArray) {
				int otherLen = otherArray.Values.Length;
				if (otherLen == 0) {
					return this;
				}

				int thisLen  = this.Values.Length;
				var newArray = new SifoscObject?[thisLen + otherLen];

				this.Values.CopyTo(newArray);
				otherArray.Values.CopyTo(newArray.AsMemory()[thisLen..]);

				return this with { Values = newArray };
			}

			return null;
		}
	}
}
