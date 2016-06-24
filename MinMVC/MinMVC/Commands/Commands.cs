using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Commands : ICommands
	{
		[Inject]
		public IContext context;

		readonly IDictionary<Type, ICommandCache> _caches = new Dictionary<Type, ICommandCache>();

		[PostInjection]
		public void Init()
		{
			context.onCleanUp += CleanUp;
		}

		void CleanUp()
		{
			context.onCleanUp -= CleanUp;
			_caches.Values.Each(cache => cache.CleanUp());
			_caches.Clear();
		}

		public ICommandCache Get<T>(bool init = false) where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);
			ICommandCache cache = _caches.Ensure (type, () => new CommandCache<T>(context, init));

			return cache;
		}

		public bool Has<T>() where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);

			return _caches.ContainsKey(type);
		}

		public void Remove<T>() where T : class, IBaseCommand, new()
		{
			Type type = typeof(T);
			ICommandCache cache;

			if(_caches.TryGetValue(type, out cache)) {
				_caches.Remove(type);
				cache.CleanUp();
			}
		}
	}
}
