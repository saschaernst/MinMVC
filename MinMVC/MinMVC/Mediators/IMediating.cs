using System;

namespace MinMVC
{
	public interface IMediating : IMediated
	{
		Action<IMediated> Mediate { get; set; }
	}
}
