using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Context : IContext
	{
		public event Action OnCleanUp = delegate { };

		public Action<string> Output { get; set; }

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
		bool optionalInjections;

		public Context (string id = null, bool optInjections = false, Action<string> output = null)
		{
			Output = output ?? Console.WriteLine;
			Id = id;
			optionalInjections = optInjections;
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

		public void Register<T> (bool preventCaching = false) where T: class, new()
		{
			Register<T, T>(preventCaching);
		}

		public void Register<TInterface, TClass> (bool preventCaching = false) where TInterface : class where TClass: class, new()
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
				throw new CannotRegisterInterfaceAsValueException(value.Name + " is an interface");
			}

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
			Register(key, null, true);
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
				if (optionalInjections) {
					Output(">>>>>>>>>> not registered: " + key);
				}
				else {
					throw new NotRegisteredException("not registered: " + key);
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
}
