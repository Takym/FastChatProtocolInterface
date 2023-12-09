/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public record SifoscObject
	{
		private static readonly ConcurrentDictionary<ulong, SifoscObject> _objects = new();
		private static          ulong                                     _next_id = 2;

		public ulong Identifier { get; }

		public SifoscObject()
		{
			switch (this) {
			case SifoscNull:
				this.Identifier = 0;
				break;
			case SifoscBoolean b when b.Value:
				this.Identifier = 1;
				break;
			case SifoscBoolean:
				this.Identifier = 2;
				break;
			default:
				do {
					this.Identifier = Interlocked.Increment(ref _next_id);
				} while (!_objects.TryAdd(this.Identifier, this));
				break;
			}

			bool succeeded = _objects.TryAdd(this.Identifier, this);
			Debug.Assert(succeeded);
		}

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
		public ReadOnlyMemory<SifoscObject?> Values { get; init; }

		protected override bool PrintMembers(StringBuilder builder)
		{
			// 参考：https://qiita.com/muniel/items/fd843abc55a5626e5c45

			bool appendComma = base.PrintMembers(builder);

			var s = this.Values.Span;
			for (int i = 0; i < s.Length; ++i) {
				if (i != 0 || appendComma) {
					builder.Append(", ");
				}
				builder.Append(s[i]);
			}
			return s.Length > 0;
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

		public static SifoscNull Instance => _inst;

		private SifoscNull() { }
	}

	public sealed record SifoscBoolean : SifoscObject
	{
		private static readonly SifoscBoolean _true  = new();
		private static readonly SifoscBoolean _false = new();

		public static SifoscBoolean TrueValue  => _true;
		public static SifoscBoolean FalseValue => _false;

		public bool Value => ReferenceEquals(this, _true);

		private SifoscBoolean() { }
	}

	public sealed record SifoscInteger : SifoscObject
	{
		public long Value { get; init; }

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
