using System;
using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	public enum InjectionCheck
	{
		None,
		Warning,
		Exception
	}
	public class Context : IContext
	{
		public event Action OnCleanUp = delegate { };

		public Action<string> Output { get; set; }

		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, Func<object>> handlerMap = new Dictionary<Type, Func<object>>();
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
		InjectionCheck handleMissingInjections;
		bool useAutoResolve;

		public Context (InjectionCheck checkInjections = InjectionCheck.Exception, bool autoResolve = false, Action<string> output = null)
		{
			Output = output ?? Console.WriteLine;
			handleMissingInjections = checkInjections;
			useAutoResolve = autoResolve;
			injector = new Injector(this);
			RegisterInstance<IContext>(this);
		}

		public Context (string id, InjectionCheck checkInjections = InjectionCheck.Exception, bool autoResolve = false, Action<string> output = null)
			: this(checkInjections, autoResolve, output)
		{
			Id = id;
		}

		public void CleanUp ()
		{
			Parent = null;

			if (OnCleanUp != null) {
				OnCleanUp();
				OnCleanUp = null;
			}
		}

		public void Register<T> (bool preventCaching = false) where T : class, new()
		{
			Register<T, T>(preventCaching);
		}

		public void Register<TInterface, TClass> (bool preventCaching = false) where TInterface : class where TClass : class, new()
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
			if (value != null && value.IsInterface) {
				throw new CannotRegisterInterfaceAsValue(value.Name + " is an interface");
			}

			if (typeMap.AddNewEntry(key, value)) {
				if (!preventCaching) {
					Cache(key, default(object));
				}
			}
			else {
				throw new AlreadyRegistered("already registered; " + key);
			}
		}

		public void RegisterInstance<T> (T instance, bool preventInjection = false)
		{
			Type key = typeof(T);
			Register(key, null, true);
			Cache(key, instance);

			if (!preventInjection) {
				forceInjections.Add(instance);
			}
		}

		public void RegisterHandler<T> (Func<object> handler)
		{
			Type key = typeof(T);
			RegisterHandler(key, handler);
		}

		public void RegisterHandler (Type key, Func<object> handler)
		{
			handlerMap.AddNewEntry(key, handler);
		}

		void Cache<T> (Type key, T instance)
		{
			if (!instanceCache.AddNewEntry(key, instance)) {
				throw new AlreadyRegistered("already cached; " + key);
			}
		}

		public T Get<T> (Type key = null)
		{
			key = key ?? typeof(T);

			return (T)GetInstance(key);
		}

		public object GetInstance (Type type)
		{
			object instance;
			instanceCache.TryGetValue(type, out instance);

			if (instance == null) {
				Type value;
				Func<object> handler;

				if (typeMap.TryGetValue(type, out value)) {
					instance = CreateInstance(type, value);
				}
				else if (handlerMap.TryGetValue(type, out handler)) {
					instance = handler();
					injector.Inject(instance);
				}
				else if (HasParent) {
					instance = parent.GetInstance(type);
				}
			}
			else if (forceInjections.Contains(instance)) {
				forceInjections.Remove(instance);
				injector.Inject(instance);
			}

			if (instance == null) {
				if (useAutoResolve && type != null && type.IsClass && !type.IsInterface && !type.IsAbstract) {
					Register(type);
					instance = GetInstance(type);
				}
				else {
					switch (handleMissingInjections) {
						case InjectionCheck.Exception:
							throw new NotRegistered("not registered: " + type);
						case InjectionCheck.Warning:
							Output("not registered: " + type);
							break;
					}
				}
			}

			return instance;
		}

		object CreateInstance (Type key, Type value)
		{
			var instance = Activator.CreateInstance(value);
			instanceCache.UpdateEntry(key, instance);
			injector.Inject(instance);

			return instance;
		}

		public void Inject<T> (T instance)
		{
			injector.Inject(instance);
		}

		public T Create<T> () where T : new()
		{
			var type = typeof(T);

			return (T)CreateInstance(type, type);
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
}
