using System;
using System.Collections.Generic;

namespace MinMVC
{
	public abstract class BaseCommands : ICommands
	{
		readonly IDictionary<Type, ICommandCache> caches = new Dictionary<Type, ICommandCache>();

		protected void ClearCaches ()
		{
			foreach (var pair in caches) {
				pair.Value.CleanUp();
			}

			caches.Clear();
		}

		public ICommandCache Get<T> (bool init = false) where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);
			ICommandCache cache = caches.Retrieve(type, () => new CommandCache<T>(this));

			return cache;
		}

		public bool Has<T> () where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);

			return caches.ContainsKey(type);
		}

		public void Remove<T> () where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);
			ICommandCache cache;

			if (caches.TryGetValue(type, out cache)) {
				caches.Remove(type);
				cache.CleanUp();
			}
		}

		public abstract IBaseCommand GetCommand<T> () where T : class, IBaseCommand, new();
	}
}
