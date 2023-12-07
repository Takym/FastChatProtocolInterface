/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public static class SifoscLexer
	{
		public static bool TryScanKeyword(this SourceCode sc, string keyword)
			=> sc.AdvanceIf(keyword) && !(sc.TryPeekChar(out char ch) && IsAlphanum(ch));

		public static bool TryScanSignedInteger(this SourceCode sc, out int result)
		{
			bool shouldRetreat = sc.TryScanSign(out bool isMinus);

			if (sc.TryScanUnsignedInteger(out result)) {
				if (isMinus) {
					result = -result;
				}
				return true;
			}

			if (shouldRetreat) {
				sc.Retreat();
			}
			return false;
		}

		public static bool TryScanSign(this SourceCode sc, out bool isMinus)
		{
			isMinus = false;

			if (sc.TryPeekChar(out char ch)) {
				switch (ch) {
				case '+':
					sc.Advance();
					return true;
				case '-':
					isMinus = true;
					goto case '+';
				}
			}

			return false;
		}

		public static bool TryScanUnsignedInteger(this SourceCode sc, out int result)
		{
			result = 0;

			if (sc.TryPeekChar(out char ch) && IsDigit(ch)) {
				do {
					result *= 10;
					result += ch - '0';
					sc.Advance();
				} while (sc.TryPeekChar(out ch) && IsDigit(ch));

				return true;
			}

			return false;
		}

		private static bool IsAlphanum(char ch) => IsAlphabet(ch) || IsDigit(ch) || ch == '_';
		private static bool IsAlphabet(char ch) => ch is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');
		private static bool IsDigit   (char ch) => ch is  >= '0' and <= '9';
	}
}
