/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System.Collections.Generic;
using System.IO;

namespace FastChatProtocolInterface.SimpleFormulaScript
{
	public sealed record SourceCode
	{
		private readonly string     _src;
		private          int        _idx;
		private readonly Stack<int> _idx_stack;

		public string Text   => _src;
		public int    Index  => _idx;
		public int    Length => _src.Length;

		public SourceCode(string src)
		{
			_src       = src;
			_idx       = 0;
			_idx_stack = new();
		}

		public void BeginScope()
			=> _idx_stack.Push(_idx);

		public void EndScope(bool restoreIndex)
		{
			int tmp = 0;
			ref int pIdx = ref (restoreIndex ? ref _idx : ref tmp);

			if (!_idx_stack.TryPop(out pIdx) && restoreIndex) {
				pIdx = 0;
			}
		}

		public bool TrySeek(int index, SeekOrigin origin)
		{
			int len = _src.Length;

			int newIndex = origin switch {
				SeekOrigin.Begin   => index,
				SeekOrigin.Current => index + _idx,
				SeekOrigin.End     => index + len,
				_                  => -1
			};

			if (0 <= newIndex && newIndex < len) {
				_idx = newIndex;
				return true;
			}

			return false;
		}

		public bool TryReadChar(out char result)
		{
			if (_idx < _src.Length) {
				result = _src[_idx++];
				return true;
			} else {
				result = '\0';
				return false;
			}
		}

		public bool TryPeekChar(out char result)
		{
			if (_idx < _src.Length) {
				result = _src[_idx++];
				return true;
			} else {
				result = '\0';
				return false;
			}
		}

		public void Advance() => ++_idx;
		public void Retreat() => --_idx;

		public bool AdvanceIf(char ch)
		{
			if (_idx < _src.Length && _src[_idx] == ch) {
				++_idx;
				return true;
			}
			return false;
		}

		public bool AdvanceIf(string pattern)
		{
			this.BeginScope();

			for (int i = 0; i < pattern.Length; ++i) {
				if (!this.AdvanceIf(pattern[i])) {
					this.EndScope(true);
					return false;
				}
			}

			this.EndScope(false);
			return true;
		}

		public void SkipSpaces()
		{
			while (_idx < _src.Length && _src[_idx] is (>= '\n' and <= '\r') or '\t' or ' ') {
				++_idx;
			}
		}
	}
}
