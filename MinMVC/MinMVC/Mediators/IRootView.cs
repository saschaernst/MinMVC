using System;

namespace MinMVC
{
	public interface IRootView : IMediatedView
	{
		Action<IMediatedView> Mediate { get; set; }
	}
}
