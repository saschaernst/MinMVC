using System;

namespace MinMVC
{
	public interface IMediated
	{
		event Action OnRemove;

		void OnMediation ();

		void Remove ();
	}
}
