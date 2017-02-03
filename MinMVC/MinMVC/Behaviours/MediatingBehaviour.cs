using System;
using System.Collections.Generic;

namespace MinMVC
{
	public abstract class MediatingBehaviour : MediatedBehaviour, IMediating
	{
		public Action<IMediated> OnMediate { get; set; }

		readonly HashSet<IMediated> waitingForMediation = new HashSet<IMediated>();

		public void Mediate (IMediated mediated)
		{
			if (OnMediate != null) {
				OnMediate(mediated);
			}
			else {
				waitingForMediation.Add(mediated);
			}
		}

		public void ResolveQueue ()
		{
			foreach (var item in waitingForMediation) {
				OnMediate(item);
			}

			waitingForMediation.Clear();
		}

		protected override void Cleanup ()
		{
			OnMediate = null;

			base.Cleanup();
		}
	}
}
