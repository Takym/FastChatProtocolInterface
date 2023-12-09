/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Text;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public sealed record SifoscViewForAllObjects : SifoscObject
	{
		private static readonly SifoscViewForAllObjects _inst = new();

		public static   SifoscViewForAllObjects Instance => _inst;
		public override string                  Code     => "allobj";

		private SifoscViewForAllObjects() { }

		protected override bool PrintMembers(StringBuilder builder)
		{
			// 参考：https://qiita.com/muniel/items/fd843abc55a5626e5c45

			bool appendComma = base.PrintMembers(builder);

			foreach (var item in EnumerateAllObjects()) {
				if (appendComma) {
					builder.Append(", ");
				}
				if (item is SifoscViewForAllObjects) {
					this.PrintMembersSimply(builder);
				} else {
					builder.Append(item);
				}
				appendComma = true;
			}

			return appendComma;
		}

		public void PrintMembersSimply(StringBuilder builder)
		{
			builder
				.Append(nameof(SifoscViewForAllObjects))
				.Append(" { ");

			if (base.PrintMembers(builder)) {
				builder.Append(' ');
			}

			builder.Append('}');
		}

		public override SifoscObject? Get(SifoscObject? other)
		{
			if (other is SifoscInteger id) {
				return GetObject(unchecked((ulong)(id.Value)));
			}

			return null;
		}
	}
}
