using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Context : IContext
	{
		public event Action onCleanUp = delegate { };

		readonly IDictionary<Type, Type> _typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, bool> _initMap = new Dictionary<Type, bool>();
		readonly IDictionary<Type, object> _instanceCache = new Dictionary<Type, object>();

		IContext _parent;
		IInjector _injector;

		public IContext parent {
			set {
				if(hasParent) {
					_parent.onCleanUp -= CleanUp;
				}

				_parent = value;

				if(hasParent) {
					_parent.onCleanUp += CleanUp;
				}
			}
		}

		public Context(IInjector injector = null)
		{
			_injector = injector ?? new Injector();
			_injector.GetInstance = GetInstance;

			RegisterInstance<IContext>(this);
		}

		public void CleanUp()
		{
			parent = null;
			onCleanUp();
		}

		public void Register<T>(bool preventCaching = false)
		{
			Register<T, T>(preventCaching);
		}

		public void Register<TInterface, TClass>(bool preventCaching = false)
		{
			Type key = typeof(TInterface);
			Type value = typeof(TClass);
			Register(key, value, preventCaching);
		}

		public void Register(Type type, bool preventCaching = false)
		{
			Register(type, type, preventCaching);
		}

		public void Register(Type key, Type value, bool preventCaching = false)
		{
			if(!_typeMap.ContainsKey(key)) {
				_typeMap[key] = value;
				_initMap [key] = false;

				if(!preventCaching) {
					Cache(key, default (object));
				}
			} else {
				throw new AlreadyRegisteredException("already registered; " + key);
			}
		}

		public void RegisterInstance<T>(T instance)
		{
			Type key = typeof(T);
			Register(key, key, true);
			Cache(key, instance);
		}

		void Cache<T>(Type key, T instance)
		{
			object cachedInstance;
			_instanceCache.TryGetValue(key, out cachedInstance);

			if(cachedInstance == null) {
				_instanceCache[key] = instance;
			} else {
				throw new AlreadyRegisteredException("already cached; " + key);
			}
		}

		public void OnInit<T>() where T : class
		{
			Type type = typeof(T);
			_initMap [type] = true;
			CheckWaiting ();
		}

		void CheckWaiting ()
		{
			
		}

		public T Get<T>(Type key = null) where T : class
		{
			key = key ?? typeof(T);

			return (T)GetInstance(key);
		}

		public object GetInstance(Type key)
		{
			object instance;
			_instanceCache.TryGetValue(key, out instance);

			if(instance == null) {
				Type value;

				if(_typeMap.TryGetValue(key, out value)) {
					instance = Activator.CreateInstance(value);
					_injector.Inject(instance);

					if(_instanceCache.ContainsKey(key)) {
						_instanceCache[key] = instance;
					}
				} else if(hasParent) {
					instance = _parent.GetInstance(key);
				}
			}

			if(instance == null) {
				throw new NotRegisteredException("not registered: " + key);
			}

			return instance;
		}

		public bool Has<T>()
		{
			Type type = typeof(T);

			return Has(type);
		}

		public bool Has(Type key)
		{
			bool hasKey = _instanceCache.ContainsKey(key) || _typeMap.ContainsKey(key);
			bool findInParent = !hasKey && hasParent;

			return findInParent ? _parent.Has(key) : hasKey;
		}

		public void Inject<T>(T instance)
		{
			_injector.Inject(instance);
		}

		bool hasParent {
			get {
				return _parent != null;
			}
		}
	}

	public class NotRegisteredException : Exception
	{
		public NotRegisteredException(string message) : base(message)
		{
		}
	}

	public class AlreadyRegisteredException : Exception
	{
		public AlreadyRegisteredException(string message) : base(message)
		{
		}
	}
}
