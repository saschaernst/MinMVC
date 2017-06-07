using System;
using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	public abstract class MediatingBehaviour : MediatedBehaviour, IMediating
	{
		readonly HashSet<IMediated> waitingForMediation = new HashSet<IMediated>();
		Action<IMediated> onMediate;

		public void SetMediateHandler (Action<IMediated> mediateHandler)
		{
			onMediate = mediateHandler;

			waitingForMediation.Each(mediated => onMediate(mediated));
			waitingForMediation.Clear();
		}

		public void Mediate (IMediated mediated)
		{
			if (onMediate != null) {
				onMediate(mediated);
			}
			else {
				waitingForMediation.Add(mediated);
			}
		}

		protected override void Cleanup ()
		{
			onMediate = null;

			base.Cleanup();
		}
	}
}
