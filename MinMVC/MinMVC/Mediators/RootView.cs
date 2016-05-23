using System;

namespace MinMVC
{
	public abstract class RootView : MediatedView, IRootView
	{
		public Action<IMediatedView> mediate { get; set; }
	}
}
