/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http.Headers;
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

		public static SifoscObject? GetObject(ulong id)
			=> _objects.TryGetValue(id, out var result) ? result : null;

		public virtual SifoscObject? Plus    ()                    => null;
		public virtual SifoscObject? Minus   ()                    => null;
		public virtual SifoscObject? Negate  ()                    => null;
		public virtual SifoscObject? Add     (SifoscObject? other) => null;
		public virtual SifoscObject? Subtract(SifoscObject? other) => null;
		public virtual SifoscObject? Multiply(SifoscObject? other) => null;
		public virtual SifoscObject? Divide  (SifoscObject? other) => null;
		public virtual SifoscObject? Modulo  (SifoscObject? other) => null;
		public virtual SifoscObject? And     (SifoscObject? other) => null;
		public virtual SifoscObject? Or      (SifoscObject? other) => null;
		public virtual SifoscObject? Xor     (SifoscObject? other) => null;
		public virtual SifoscObject? Get     (SifoscObject? other) => null;
	}

	public sealed record SifoscNull : SifoscObject
	{
		private static readonly SifoscNull _inst = new();

		public static   SifoscNull Instance => _inst;
		public override string     Code     => "null";

		private SifoscNull() { }
	}
}
