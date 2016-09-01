using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Commands : ICommands
	{
		[Inject]
		public IContext context;

		readonly IDictionary<Type, ICommandCache> caches = new Dictionary<Type, ICommandCache>();

		[PostInjection]
		public void Init ()
		{
			context.OnCleanUp += CleanUp;
		}

		void CleanUp ()
		{
			context.OnCleanUp -= CleanUp;
			caches.EachValue(cache => cache.CleanUp());
			caches.Clear();
		}

		public ICommandCache Get<T> (bool init = false) where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);
			ICommandCache cache = caches.Retrieve(type, () => new CommandCache<T>(context));

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
	}
}
