using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	public class NamedDispatcher : INamedDispatcher
	{
		static readonly IList<ICommandCache> EMPTY_CACHE = new List<ICommandCache>();

		[Inject]
		public ICommands commands;

		readonly IDictionary<string, IList<ICommandCache>> eventMap = new Dictionary<string, IList<ICommandCache>>();

		public void Register<T> (string eventName) where T : class, IBaseCommand, new()
		{
			ICommandCache cache = commands.Get<T>();
			eventMap.Retrieve(eventName, CreateList).Add(cache);
		}

		IList<ICommandCache> CreateList ()
		{
			return new List<ICommandCache>();
		}

		public void Unregister<T> (string eventName) where T : class, IBaseCommand, new()
		{
			IList<ICommandCache> caches;

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
			var caches = GetCaches(eventName);

			for (int i = 0; i < caches.Count; i++) {
				caches[i].Execute();
			}
		}

		public void Execute<TParam> (string eventName, TParam param)
		{
			var caches = GetCaches(eventName);

			for (int i = 0; i < caches.Count; i++) {
				caches[i].Execute(param);
			}
		}

		public void Execute<TParam0, TParam1> (string eventName, TParam0 param0, TParam1 param1)
		{
			var caches = GetCaches(eventName);

			for (int i = 0; i < caches.Count; i++) {
				caches[i].Execute(param0, param1);
			}
		}

		IList<ICommandCache> GetCaches (string eventName)
		{
			IList<ICommandCache> caches;
			bool hasCaches = eventMap.TryGetValue(eventName, out caches);

			return hasCaches ? caches : EMPTY_CACHE;
		}
	}
}
