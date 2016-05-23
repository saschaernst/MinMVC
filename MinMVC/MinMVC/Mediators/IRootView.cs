using System;

namespace MinMVC
{
	public interface IRootView : IMediatedView
	{
		Action<IMediatedView> mediate { get; set; }
	}
}
