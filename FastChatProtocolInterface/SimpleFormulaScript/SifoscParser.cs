/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public static class SifoscParser
	{
		public static bool TryParse(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			if (sc.TryParseExpression(out result)) {
				sc.SkipSpaces();
				return sc.Index >= sc.Length;
			}

			return false;
		}

		// TODO: TryParseStatement

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParseExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
			=> sc.TryParseAddSubExpression(out result);

		public static bool TryParseAddSubExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			result = null;
			ref var left   = ref result;
			char    op     = '\0';

			while (sc.TryParseMulDivExpression(out var right)) {
				switch (op) {
				case '\0' when left is null:
					left = right;
					break;
				case '+':
					left = left?.Add(right);
					break;
				case '-':
					left = left?.Subtract(right);
					break;
				default:
					left = null;
					goto end;
				}

				sc.SkipSpaces();
				if (!sc.TryReadChar(out op) || op is not ('+' or '-')) {
					if (op != '\0') {
						sc.Retreat();
					}
					break;
				}
			}

		end:
			if (result is null) {
				sc.EndScope(true);
				return false;
			} else {
				sc.EndScope(false);
				return true;
			}
		}

		public static bool TryParseMulDivExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			result = null;
			ref var left   = ref result;
			char    op     = '\0';

			while (sc.TryParseValueExpression(out var right)) {
				switch (op) {
				case '\0' when left is null:
					left = right;
					break;
				case '*':
					left = left?.Multiply(right);
					break;
				case '/':
					left = left?.Divide(right);
					break;
				case '%':
					left = left?.Modulo(right);
					break;
				default:
					left = null;
					goto end;
				}

				sc.SkipSpaces();
				if (!sc.TryReadChar(out op) || op is not ('*' or '/' or '%')) {
					if (op != '\0') {
						sc.Retreat();
					}
					break;
				}
			}

		end:
			if (result is null) {
				sc.EndScope(true);
				return false;
			} else {
				sc.EndScope(false);
				return true;
			}
		}

		public static bool TryParseValueExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.SkipSpaces();

			return sc.TryParseParenthesisExpression(out result)
				|| sc.TryParseSignExpression       (out result)
				|| sc.TryParseValueLiteral         (out result);
		}

		public static bool TryParseParenthesisExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			if (sc.AdvanceIf('(') && sc.TryParseExpression(out result)) {
				sc.SkipSpaces();
				if (sc.AdvanceIf(')')) {
					sc.EndScope(false);
					return true;
				}
			}

			result = null;
			sc.EndScope(true);
			return false;
		}

		public static bool TryParseSignExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			if (sc.TryPeekChar(out char ch) && ch is '+' or '-') {
				sc.Advance();

				if (sc.TryParseValueExpression(out var obj)) {
					result = ch == '+' ? obj.Plus() : obj.Minus();

					if (result is null) {
						goto fail;
					} else {
						goto succeed;
					}
				}

				//sc.Retreat();
			}

			result = null;

		fail:
			sc.EndScope(true);
			return false;

		succeed:
			sc.EndScope(false);
			return true;
		}

		public static bool TryParseValueLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			if (TryParseArrayLiteral(sc, out var aryVal)) {
				result = aryVal;
				return true;
			} else if (TryParseNullLiteral(sc, out var nulVal)) {
				result = nulVal;
				return true;
			} else if (TryParseNewObjectLiteral(sc, out var objVal)) {
				result = objVal;
				return true;
			} else if (TryParseIntegerLiteral(sc, out var intVal)) {
				result = intVal;
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseArrayLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscArray? result)
		{
			sc.BeginScope();

			if (sc.AdvanceIf('[')) {
				var list = new List<SifoscObject>();

				while (sc.TryParseExpression(out var item)) {
					list.Add(item);

					sc.SkipSpaces();
					if (!sc.AdvanceIf(',')) {
						break;
					}
				}

				sc.SkipSpaces();
				if (sc.AdvanceIf(']')) {
					result = new() { Values = list.ToArray() };
					sc.EndScope(false);
					return true;
				}
			}

			result = null;
			sc.EndScope(true);
			return false;
		}

		public static bool TryParseNullLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscNull? result)
		{
			if (sc.TryScanKeyword("null")) {
				result = SifoscNull.Instance;
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseNewObjectLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			if (sc.TryScanKeyword("newobj")) {
				result = new();
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseIntegerLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscInteger? result)
		{
			if (sc.TryScanSignedInteger(out long value)) {
				result = new() { Value = value };
				return true;
			} else {
				result = null;
				return false;
			}
		}
	}
}
