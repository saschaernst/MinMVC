using System;

namespace MinMVC
{
	public abstract class MediatedRoot : MediatedBehaviour, IMediatedRoot
	{
		public Action<IMediated> Mediate { get; set; }
	}
}
