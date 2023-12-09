/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public record SifoscObject
	{
		private static readonly ConcurrentDictionary<ulong, SifoscObject> _objects = [];
		private static          ulong                                     _next_id = 3;

		public         ulong  Identifier { get; }
		public virtual string Code       => "newobj";

		public SifoscObject()
		{
			this.Identifier = this.CreateIdentifier();
		}

		protected SifoscObject(SifoscObject _)
		{
			this.Identifier = this.CreateIdentifier();
		}

		private ulong CreateIdentifier()
		{
			ulong? id = null;

			do {
				id = this switch {
					SifoscNull              => id is null ? 0UL :                 throw new InvalidOperationException(),
					SifoscViewForAllObjects => id is null ? 1UL :                 throw new InvalidOperationException(),
					SifoscBoolean           => id is null ? 2UL : id == 2 ? 3UL : throw new InvalidOperationException(),
					_                       => Interlocked.Increment(ref _next_id)
				};
			} while (!_objects.TryAdd(id.Value, this));

			return id.Value;
		}

		public static IEnumerable<SifoscObject> EnumerateAllObjects()
			=> _objects.Values;

		public virtual SifoscObject? Plus    ()                    => null;
		public virtual SifoscObject? Minus   ()                    => null;
		public virtual SifoscObject? Add     (SifoscObject? other) => null;
		public virtual SifoscObject? Subtract(SifoscObject? other) => null;
		public virtual SifoscObject? Multiply(SifoscObject? other) => null;
		public virtual SifoscObject? Divide  (SifoscObject? other) => null;
		public virtual SifoscObject? Modulo  (SifoscObject? other) => null;
	}

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

				this      .Values.CopyTo(newArray);
				otherArray.Values.CopyTo(newArray.AsMemory()[thisLen..]);

				return this with { Values = newArray };
			}

			return null;
		}
	}

	public sealed record SifoscNull : SifoscObject
	{
		private static readonly SifoscNull _inst = new();

		public static   SifoscNull Instance => _inst;
		public override string     Code     => "null";

		private SifoscNull() { }
	}

	public sealed record SifoscViewForAllObjects : SifoscObject
	{
		private static readonly SifoscViewForAllObjects _inst = new();

		public static   SifoscViewForAllObjects Instance => _inst;
		public override string                  Code     => "allobj";

		private SifoscViewForAllObjects() { }

		protected override bool PrintMembers(StringBuilder builder)
		{
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
	}

	public sealed record SifoscBoolean : SifoscObject
	{
		private static readonly SifoscBoolean _true  = new();
		private static readonly SifoscBoolean _false = new();

		public static   SifoscBoolean TrueValue  => _true;
		public static   SifoscBoolean FalseValue => _false;
		public          bool          Value      => ReferenceEquals(this, _true);
		public override string        Code       => this.Value ? "true" : "false";

		private SifoscBoolean() { }
	}

	public sealed record SifoscInteger : SifoscObject
	{
		public          long   Value { get; init; }
		public override string Code  => this.Value.ToString();

		public override SifoscObject? Plus()
			=> this;

		public override SifoscObject? Minus()
			=> this with { Value = -this.Value };

		public override SifoscObject? Add(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value + otherInt.Value };
			}

			return null;
		}

		public override SifoscObject? Subtract(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value - otherInt.Value };
			}

			return null;
		}

		public override SifoscObject? Multiply(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value * otherInt.Value };
			}

			return null;
		}

		public override SifoscObject? Divide(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value / otherInt.Value };
			}

			return null;
		}

		public override SifoscObject? Modulo(SifoscObject? other)
		{
			if (other is SifoscInteger otherInt) {
				return this with { Value = this.Value % otherInt.Value };
			}

			return null;
		}
	}
}
