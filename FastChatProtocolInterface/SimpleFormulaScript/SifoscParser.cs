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
			if (sc.TryParseStatement(out result)) {
				sc.SkipSpaces();
				return sc.Index >= sc.Length;
			}

			return false;
		}

		public static bool TryParseStatement(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			if (sc.TryParseExpression(out result)) {
				sc.SkipSpaces();
				sc.AdvanceIf(';');
				return true;
			}

			return false;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool TryParseExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
			=> sc.TryParseLogicalExpression(out result);

		public static bool TryParseLogicalExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			result = null;
			ref var left   = ref result;
			char    op     = '\0';

			while (sc.TryParseAddSubExpression(out var right)) {
				switch (op) {
				case '\0' when left is null:
					left = right;
					break;
				case '&':
					left = left?.And(right);
					break;
				case '|':
					left = left?.Or(right);
					break;
				case '^':
					left = left?.Xor(right);
					break;
				default:
					left = null;
					goto end;
				}

				sc.SkipSpaces();
				if (!sc.TryReadChar(out op) || op is not ('&' or '|' or '^')) {
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
				|| sc.TryParseGetExpression        (out result)
				|| sc.TryParseSignExpression       (out result)
				|| sc.TryParseNotExpression        (out result)
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

		public static bool TryParseGetExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			if (sc.TryParseValueExpression(out var left)) {
				sc.SkipSpaces();
				if (sc.AdvanceIf('.') && sc.TryParseValueExpression(out var right)) {
					result = left.Get(right);

					if (result is null) {
						goto fail;
					} else {
						goto succeed;
					}
				}
			}

			result = null;

		fail:
			sc.EndScope(true);
			return false;

		succeed:
			sc.EndScope(false);
			return true;
		}

		public static bool TryParseSignExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			if (sc.TryReadChar            (out char ch ) && ch is '+' or '-' &&
				sc.TryParseValueExpression(out var  obj)
			) {
				result = ch == '+' ? obj.Plus() : obj.Minus();

				if (result is null) {
					goto fail;
				} else {
					goto succeed;
				}
			}

			result = null;

		fail:
			sc.EndScope(true);
			return false;

		succeed:
			sc.EndScope(false);
			return true;
		}

		public static bool TryParseNotExpression(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscObject? result)
		{
			sc.BeginScope();

			if (sc.TryReadChar            (out char ch ) && ch is '!' &&
				sc.TryParseValueExpression(out var  obj)
			) {
				result = obj.Negate();

				if (result is null) {
					goto fail;
				} else {
					goto succeed;
				}
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
			if (sc.TryParseArrayLiteral(out var aryVal)) {
				result = aryVal;
				return true;
			} else if (sc.TryParseNullLiteral(out var nulVal)) {
				result = nulVal;
				return true;
			} else if (sc.TryParseNewObjectLiteral(out var objVal)) {
				result = objVal;
				return true;
			} else if (sc.TryParseAllObjectsLiteral(out var allVal)) {
				result = allVal;
				return true;
			} else if (sc.TryParseBooleanLiteral(out var flgVal)) {
				result = flgVal;
				return true;
			} else if (sc.TryParseIntegerLiteral(out var intVal)) {
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

		public static bool TryParseAllObjectsLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscViewForAllObjects? result)
		{
			if (sc.TryScanKeyword("allobj")) {
				result = SifoscViewForAllObjects.Instance;
				return true;
			} else {
				result = null;
				return false;
			}
		}

		public static bool TryParseBooleanLiteral(this SourceCode sc, [NotNullWhen(true)][MaybeNullWhen(false)] out SifoscBoolean? result)
		{
			if (sc.TryScanKeyword("true")) {
				result = SifoscBoolean.TrueValue;
				return true;
			} else if (sc.TryScanKeyword("false")) {
				result = SifoscBoolean.FalseValue;
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
