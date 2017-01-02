using System.Collections.Generic;

namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediated
	{
		protected T mediated;

		readonly IDictionary<ISignal, object> signalMap = new Dictionary<ISignal, object>();

		public void Init (IMediated med)
		{
			mediated = (T)med;
			mediated.OnStart += OnStart;
			mediated.OnRemove += OnRemove;

			Register();
		}

		protected abstract void Register ();

		protected void RegisterSignal (ISignal signal, object listener)
		{
			signal.Add(listener);
			signalMap.AddNewEntry(signal, listener);
		}

		protected virtual void OnStart ()
		{
			
		}

		protected void OnRemove ()
		{
			Unregister();

			ClearSignals();

			mediated.OnRemove -= OnRemove;
			mediated = null;
		}

		protected virtual void Unregister ()
		{

		}

		void ClearSignals ()
		{
			foreach (var item in signalMap) {
				item.Key.Remove(item.Value);
			}

			signalMap.Clear();
		}
	}
}
