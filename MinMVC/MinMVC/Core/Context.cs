using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Context : IContext
	{
		//public const string ROOT = "root";

		//public static IContext Root {
		//	get {
		//		return Get(ROOT);
		//	}
		//}

		//readonly static IDictionary<string, IContext> contexts = new Dictionary<string, IContext>();

		//public static IContext Get (string id)
		//{
		//	return contexts[id];
		//}

		//public static IContext Add (string id, IContext context = null)
		//{
		//	return contexts[id] = context ?? new Context(id);
		//}

		//public static bool Has (string id)
		//{
		//	return contexts.ContainsKey(id);
		//}

		//public static IContext Ensure (string id)
		//{
		//	return Has(id) ? Get(id) : Add(id);
		//}

		//public static bool Remove (string id)
		//{
		//	var hasContext = Has(id);

		//	if (hasContext) {
		//		Get(id).Remove();
		//	}

		//	return hasContext;
		//}

		public event Action OnCleanUp = delegate { };

		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();
		readonly HashSet<object> forceInjections = new HashSet<object>();

		public string Id { get; private set; }

		IContext parent;

		public IContext Parent {
			get {
				return parent;
			}

			set {
				if (HasParent) {
					parent.OnCleanUp -= CleanUp;
				}

				parent = value;

				if (HasParent) {
					parent.OnCleanUp += CleanUp;
				}
			}
		}

		bool HasParent {
			get { return parent != null; }
		}

		Injector injector;
		bool enforceInjections;

		public Context (string id = "", IContext parent = null, bool enforceInjections = false)
		{
			Id = id;
			Parent = parent;

			this.enforceInjections = enforceInjections;
			injector = new Injector(this);
			RegisterInstance<IContext>(this);
		}

		public void CleanUp ()
		{
			Parent = null;

			if (OnCleanUp != null) {
				OnCleanUp();
				OnCleanUp = null;
			}
		}

		public void Register<T> (bool preventCaching = false)
		{
			Register<T, T>(preventCaching);
		}

		public void Register<TInterface, TClass> (bool preventCaching = false)
		{
			Type key = typeof(TInterface);
			Type value = typeof(TClass);

			Register(key, value, preventCaching);
		}

		public void Register (Type type, bool preventCaching = false)
		{
			Register(type, type, preventCaching);
		}

		public void Register (Type key, Type value, bool preventCaching = false)
		{
			if (typeMap.AddNewEntry(key, value)) {
				if (!preventCaching) {
					Cache(key, default(object));
				}
			}
			else {
				throw new AlreadyRegisteredException("already registered; " + key);
			}
		}

		public void RegisterInstance<T> (T instance, bool preventInjection = false)
		{
			Type key = typeof(T);
			Register(key, key, true);
			Cache(key, instance);

			if (!preventInjection) {
				forceInjections.Add(instance);
			}
		}

		void Cache<T> (Type key, T instance)
		{
			if (!instanceCache.AddNewEntry(key, instance)) {
				throw new AlreadyRegisteredException("already cached; " + key);
			}
		}

		public T Get<T> (Type key = null)
		{
			key = key ?? typeof(T);

			return (T)GetInstance(key);
		}

		public object GetInstance (Type key)
		{
			object instance;
			instanceCache.TryGetValue(key, out instance);

			if (instance == null) {
				Type value;

				if (typeMap.TryGetValue(key, out value)) {
					instance = CreateInstance(key, value);
				}
				else if (HasParent) {
					instance = parent.GetInstance(key);
				}
			}
			else if (forceInjections.Contains(instance)) {
				injector.Inject(instance);
				forceInjections.Remove(instance);
			}

			if (instance == null) {
				if (enforceInjections) {
					throw new NotRegisteredException("not registered: " + key);
				}
				else {
					Console.WriteLine(">>>>>>>>>> not registered: " + key);
				}
			}

			return instance;
		}

		object CreateInstance (Type key, Type value)
		{
			object instance = Activator.CreateInstance(value);
			instanceCache.UpdateEntry(key, instance);
			injector.Inject(instance);

			return instance;
		}

		public void Inject<T> (T instance)
		{
			injector.Inject(instance);
		}

		public bool Has<T> ()
		{
			return Has(typeof(T));
		}

		public bool Has (Type key)
		{
			bool hasKey = typeMap.ContainsKey(key);
			bool findInParent = !hasKey && HasParent;

			return findInParent ? parent.Has(key) : hasKey;
		}
	}

	public class NotRegisteredException : Exception
	{
		public NotRegisteredException (string message) : base(message)
		{
		}
	}

	public class AlreadyRegisteredException : Exception
	{
		public AlreadyRegisteredException (string message) : base(message)
		{
		}
	}
}
