using System;

namespace MinMVC
{
	public interface IMediating : IMediated
	{
		void SetMediateHandler (Action<IMediated> mediate);

		void Mediate (IMediated mediated);
	}
}
