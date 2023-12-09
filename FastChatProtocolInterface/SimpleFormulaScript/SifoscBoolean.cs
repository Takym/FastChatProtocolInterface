/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public sealed record SifoscBoolean : SifoscObject
	{
		private static readonly SifoscBoolean _true  = new();
		private static readonly SifoscBoolean _false = new();

		public static   SifoscBoolean TrueValue  => _true;
		public static   SifoscBoolean FalseValue => _false;
		public          bool          Value      => ReferenceEquals(this, _true);
		public override string        Code       => this.Value ? "true" : "false";

		private SifoscBoolean() { }

		public override SifoscObject? Negate()
			=> this.Value ? _false : _true;

		public override SifoscObject? And(SifoscObject? other)
		{
			if (other is SifoscBoolean otherFlg) {
				return this.Value && otherFlg.Value ? _true : _false;
			}

			return null;
		}

		public override SifoscObject? Or(SifoscObject? other)
		{
			if (other is SifoscBoolean otherFlg) {
				return this.Value || otherFlg.Value ? _true : _false;
			}

			return null;
		}

		public override SifoscObject? Xor(SifoscObject? other)
		{
			if (other is SifoscBoolean otherFlg) {
				return this.Value != otherFlg.Value ? _true : _false;
			}

			return null;
		}
	}
}
