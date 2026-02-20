using System;

namespace Saving
{
	[Flags]
	public enum eFile
	{
		Settings = 1,
		Shared = 2,
		Scene = 4,
		All = Scene | Shared | Settings,
	}
}