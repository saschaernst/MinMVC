using System;

namespace MinMVC
{
	public interface IMediating : IMediated
	{
		Action<IMediated> OnMediate { get; set; }

		void Mediate(IMediated mediated);

		void ResolveQueue();
	}
}
