using System.Collections.Generic;

namespace MinMVC
{
	public class NamedDispatcher : INamedDispatcher
	{
		static readonly HashSet<ICommandCache> EMPTY_CACHE = new HashSet<ICommandCache>();

		[Inject]
		public ICommands commands;

		readonly IDictionary<string, HashSet<ICommandCache>> eventMap = new Dictionary<string, HashSet<ICommandCache>>();

		public void Register<T> (string eventName) where T : class, IBaseCommand, new()
		{
			ICommandCache cache = commands.Get<T>();
			eventMap.Retrieve(eventName).Add(cache);
		}

		public void Unregister<T> (string eventName) where T : class, IBaseCommand, new()
		{
			HashSet<ICommandCache> caches;

			if (eventMap.TryGetValue(eventName, out caches)) {
				ICommandCache cache = commands.Get<T>();
				caches.Remove(cache);
			}
		}

		public void UnregisterAll (string eventName)
		{
			eventMap.Remove(eventName);
		}

		public void Execute (string eventName)
		{
			GetCache(eventName).Each(cache => cache.Execute());
		}

		public void Execute<TParam> (string eventName, TParam param)
		{
			GetCache(eventName).Each(cache => cache.Execute(param));
		}

		public void Execute<TParam0, TParam1> (string eventName, TParam0 param0, TParam1 param1)
		{
			GetCache(eventName).Each(cache => cache.Execute(param0, param1));
		}

		HashSet<ICommandCache> GetCache (string eventName)
		{
			HashSet<ICommandCache> caches;
			bool hasCaches = eventMap.TryGetValue(eventName, out caches);

			return hasCaches ? caches : EMPTY_CACHE;
		}
	}
}
