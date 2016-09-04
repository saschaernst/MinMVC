using System;

namespace MinMVC
{
	public interface IMediatedRoot : IMediated
	{
		Action<IMediated> Mediate { get; set; }
	}
}
