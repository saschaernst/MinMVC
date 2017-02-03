using System;
using System.Collections.Generic;

namespace MinMVC
{
	public static class Extensions
	{
		public static void Each<T> (this IEnumerable<T> enumerable, Action<T> handler)
		{
			foreach (T item in enumerable) {
				handler(item);
			}
		}

		public static TValue Retrieve<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			return Retrieve(dictionary, key, Activator.CreateInstance<TValue>);
		}

		public static TValue Retrieve<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> defFunc)
		{
			TValue value;

			if (!dictionary.TryGetValue(key, out value)) {
				dictionary[key] = value = defFunc();
			}

			return value;
		}

		public static bool AddNewEntry<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			bool isKeyNew = !dictionary.ContainsKey(key);

			if (isKeyNew) {
				dictionary[key] = value;
			}

			return isKeyNew;
		}

		public static bool UpdateEntry<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			bool isKeyExisting = dictionary.ContainsKey(key);

			if (isKeyExisting) {
				dictionary[key] = value;
			}

			return isKeyExisting;
		}

		public static TValue Withdraw<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key)
		{
			TValue value;

			if (dictionary.TryGetValue(key, out value)) {
				dictionary.Remove(key);
			}

			return value;
		}

		public static bool IsEmpty<T> (this ICollection<T> collection)
		{
			return collection.Count == 0;
		}

		internal static T Pop<T> (this IList<T> list)
		{
			T item = list[0];
			list.RemoveAt(0);

			return item;
		}
	}
}
