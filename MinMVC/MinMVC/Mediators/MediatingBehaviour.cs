using System;

namespace MinMVC
{
	public abstract class MediatingBehaviour : MediatedBehaviour, IMediating
	{
		public Action<IMediated> Mediate { get; set; }
	}
}
