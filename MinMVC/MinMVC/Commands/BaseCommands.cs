using System;
using System.Collections.Generic;
using MinTools;

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
			return caches.Retrieve(typeof(T), CreateCache<T>);
		}

		ICommandCache CreateCache<T> () where T : class, IBaseCommand, new()
		{
			return new CommandCache<T>(this);
		}

		public bool Has<T> () where T : class, IBaseCommand, new()
		{
			return caches.ContainsKey(typeof(T));
		}

		public void Remove<T> () where T : class, IBaseCommand, new()
		{
			var type = typeof(T);
			ICommandCache cache;

			if (caches.TryGetValue(type, out cache)) {
				caches.Remove(type);
				cache.CleanUp();
			}
		}

		public abstract IBaseCommand GetCommand<T> () where T : class, IBaseCommand, new();
	}
}
