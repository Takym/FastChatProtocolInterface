/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

namespace FastChatProtocolInterface.SimpleFormulaScript
{
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
