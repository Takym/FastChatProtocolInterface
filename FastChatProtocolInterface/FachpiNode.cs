/****
 * FACHPI: Fast Chat Protocol/Interface
 * Copyright (C) 2023 Takym.
 *
 * distributed under the MIT License.
****/

namespace FastChatProtocolInterface
{
	public abstract class FachpiNode
	{
		private readonly string? _name;

		public string Name
		{
			get  => _name         ?? string.Empty;
			init => _name = value ?? string.Empty;
		}
	}
}
