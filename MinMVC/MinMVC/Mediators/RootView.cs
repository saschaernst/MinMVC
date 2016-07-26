using System;

namespace MinMVC
{
	public abstract class RootView : MediatedView, IRootView
	{
		public Action<IMediatedView> Mediate { get; set; }
	}
}
