/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public static class SifoscParser
	{
		// TODO: TryParseStatement

		/*
		public static bool TryParseExpression(this SourceCode sc)
		{
			return false;
		}

		public static bool TryParseAddSubExpression(this SourceCode sc)
		{
			return false;
		}
		//*/

		public static bool TryParseSignExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.SkipSpaces();
			if (sc.TryPeekChar(out char ch) && ch is '+' or '-') {
				sc.Advance();

				if (sc.TryParseValue(out var obj)) {
					result = ch == '+' ? obj.Plus() : obj.Minus();
					return true;
				}

				sc.Retreat();
			}

			result = null;
			return false;
		}

		public static bool TryParseValue(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.SkipSpaces();

			if (TryParseArray(sc, out var aryVal)) {
				result = aryVal;
				return true;
			} else if (TryParseNull(sc, out var nulVal)) {
				result = nulVal;
				return true;
			} else if (TryParseNewObject(sc, out var objVal)) {
				result = objVal;
				return true;
			} else if (TryParseInteger(sc, out var intVal)) {
				result = intVal;
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseArray(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscArray? result)
		{
			sc.BeginScope();

			if (sc.AdvanceIf('[')) {
				var list = new List<SifoscObject>();

				while (sc.TryParseValue(out var item)) {
					list.Add(item);

					sc.SkipSpaces();
					if (!sc.AdvanceIf(',')) {
						break;
					}
				}

				sc.SkipSpaces();
				if (sc.AdvanceIf(']')) {
					result = new() { Values = list.ToArray() };
					return true;
				}
			}

			result = null;
			sc.EndScope(true);
			return false;
		}

		public static bool TryParseNull(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscNull? result)
		{
			if (sc.TryScanKeyword("null")) {
				result = SifoscNull.Instance;
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseNewObject(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			if (sc.TryScanKeyword("newobj")) {
				result = new();
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseInteger(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscInteger? result)
		{
			if (sc.TryScanSignedInteger(out int value)) {
				result = new() { Value = value };
				return true;
			} else {
				result = null;
				return false;
			}
		}
	}
}
