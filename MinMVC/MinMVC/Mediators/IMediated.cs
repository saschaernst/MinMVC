using System;

namespace MinMVC
{
	public interface IMediated
	{
		event Action OnStart;
		event Action<bool> OnActivated;
		event Action OnRemove;

		void OnMediation ();

		void Remove ();
	}
}
