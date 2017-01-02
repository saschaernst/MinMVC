using System;
using System.Collections.Generic;

namespace MinMVC
{
	public interface ISignal
	{
		bool Add (object listener);

		bool Remove (object listener);

		void Reset ();
	}

	public class Signal : ISignal
	{
		readonly HashSet<Action> listeners = new HashSet<Action>();

		public bool Add (object listener)
		{
			return Add((Action)listener);
		}

		public bool Add (Action listener)
		{
			return listeners.Add(listener);
		}

		public bool Remove (object listener)
		{
			return Remove((Action)listener);
		}

		public bool Remove (Action listener)
		{
			return listeners.Remove(listener);
		}

		public void Call ()
		{
			foreach (var listener in listeners) {
				listener();
			}
		}

		public void Reset ()
		{
			listeners.Clear();
		}
	}

	public class Signal<T> : ISignal
	{
		readonly HashSet<Action<T>> listeners = new HashSet<Action<T>>();

		public bool Add (object listener)
		{
			return Add((Action<T>)listener);
		}

		public bool Add (Action<T> listener)
		{
			return listeners.Add(listener);
		}

		public bool Remove (object listener)
		{
			return Remove((Action<T>)listener);
		}

		public bool Remove (Action<T> listener)
		{
			return listeners.Remove(listener);
		}

		public void Call (T p1)
		{
			foreach (var listener in listeners) {
				listener(p1);
			}
		}

		public void Reset ()
		{
			listeners.Clear();
		}
	}

	public class Signal<T, U> : ISignal
	{
		readonly HashSet<Action<T, U>> listeners = new HashSet<Action<T, U>>();

		public bool Add (object listener)
		{
			return Add((Action<T, U>)listener);
		}

		public bool Add (Action<T, U> listener)
		{
			return listeners.Add(listener);
		}

		public bool Remove (object listener)
		{
			return Remove((Action<T, U>)listener);
		}

		public bool Remove (Action<T, U> listener)
		{
			return listeners.Remove(listener);
		}

		public void Call (T p1, U p2)
		{
			foreach (var listener in listeners) {
				listener(p1, p2);
			}
		}

		public void Reset ()
		{
			listeners.Clear();
		}
	}
}
