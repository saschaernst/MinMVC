using System;
using System.Collections.Generic;

namespace MinMVC
{
	public static class Extensions
	{
		public static void Times (this int count, Action<int> handler)
		{
			for (int i = 0; i < count; i += 1) {
				handler(i);
			}
		}

		public static void Each<T> (this IEnumerable<T> enumerable, Action<T> handler)
		{
			foreach (T item in enumerable) {
				handler(item);
			}
		}

		public static TValue Ensure<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return Ensure (dictionary, key, () => Activator.CreateInstance<TValue>());
		}

		public static TValue Ensure<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defFunc)
		{
			TValue value;

			if (!dictionary.TryGetValue (key, out value)) {
				dictionary [key] = value = defFunc ();
			}

			return value;
		}
	}
}
