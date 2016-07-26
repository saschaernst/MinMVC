using System;

namespace MinMVC
{
	public interface IMediatedView
	{
		event Action OnRemove;

		void OnMediation ();

		void Remove ();
	}
}
