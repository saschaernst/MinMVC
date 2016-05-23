using System;

namespace MinMVC
{
	public interface IMediatedView
	{
		event Action onRemove;

		void OnMediation();

		void Remove();
	}
}
