using System;
using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediated
	{
		protected T mediated;

		readonly IList<Action> signals = new List<Action>();

		public void Init (IMediated med)
		{
			mediated = (T)med;

			Register();
		}

		protected virtual void Register ()
		{

		}

		protected void RegisterSignal (MinSignal signal, Action listener)
		{
			signal.Add(listener);
			signals.Add(() => signal.Remove(listener));
		}

		protected void RegisterSignal<U> (MinSignal<U> signal, Action<U> listener)
		{
			signal.Add(listener);
			signals.Add(() => signal.Remove(listener));
		}

		protected void RegisterSignal<U, V> (MinSignal<U, V> signal, Action<U, V> listener)
		{
			signal.Add(listener);
			signals.Add(() => signal.Remove(listener));
		}

		protected void Cleanup ()
		{
			Unregister();
			RemoveSignals();
		}

		protected virtual void Unregister ()
		{

		}

		void RemoveSignals ()
		{
			signals.Each(s => s());
			signals.Clear();
		}
	}
}
