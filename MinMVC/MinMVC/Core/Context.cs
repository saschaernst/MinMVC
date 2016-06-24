using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	public class Context : IContext
	{
		public event Action onCleanUp = delegate { };

		static readonly object[] EMPTY_PARAMS = new object [0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();

		readonly IDictionary<object, bool> initMap = new Dictionary<object, bool>();
		readonly IDictionary<object, HashSet<object>> waitingForInjections = new Dictionary<object, HashSet<object>> ();
		readonly IDictionary<object, HashSet<object>> injectionsForInstances = new Dictionary<object, HashSet<object>> ();

		IContext _parent;

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

		public Context()
		{
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
			if(!typeMap.ContainsKey(key)) {
				typeMap[key] = value;

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
			instanceCache.TryGetValue(key, out cachedInstance);

			if(cachedInstance == null) {
				instanceCache[key] = instance;
			} else {
				throw new AlreadyRegisteredException("already cached; " + key);
			}
		}

		public void OnStartInit(object instance)
		{
			initMap [instance] = true;
		}

		public void OnInitDone(object instance)
		{
			initMap.Remove (instance);
			CheckWaitingList (instance);
		}

		void CheckWaitingList (object instanceJustInitialized)
		{
			Dictionary<object, InjectionInfo> initList = null;

//			waitingForInjections.Each (entry => {
//				HashSet<object> instancesToWaitFor = entry.Value;
//
//				if (instancesToWaitFor.Contains (instanceJustInitialized)) {
//					bool readyToInit = true;
//
//					foreach (var instanceToWaitFor in instancesToWaitFor) {
//						if (!initMap[instanceToWaitFor]) {
//							readyToInit = false;
//							break;
//						}
//					}
//
//					if (readyToInit) {
//						initList = initList ?? new Dictionary<object, InjectionInfo> ();
//						initList[entry.Key] = info;
//					}
//				}
//			});

			initList.Each (entry => waitingForInjections.Remove (entry.Key));
			initList.Each (entry => InvokeMethods (entry.Key, entry.Value.postInits));
		}

		public T Get<T>(Type key = null) where T : class
		{
			key = key ?? typeof(T);

			return (T)GetInstance(key);
		}

		public object GetInstance(Type key)
		{
			object instance;
			instanceCache.TryGetValue(key, out instance);

			if(instance == null) {
				Type value;

				if(typeMap.TryGetValue(key, out value)) {
					instance = Activator.CreateInstance(value);
					Inject(instance);

					if(instanceCache.ContainsKey(key)) {
						instanceCache[key] = instance;
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
			bool hasKey = instanceCache.ContainsKey(key) || typeMap.ContainsKey(key);
			bool findInParent = !hasKey && hasParent;

			return findInParent ? _parent.Has(key) : hasKey;
		}

		bool hasParent {
			get {
				return _parent != null;
			}
		}

		public void Inject<T>(T instance)
		{
			Type key = instance.GetType();
			InjectionInfo info = infoMap.Ensure (key, () => ParseInfo(key));

			if(info != null) {
				HashSet<object> waitingInjections = InjectInstances(instance, key, info.injections, BindingFlags.SetProperty | BindingFlags.SetField, info.waitingFor);

				if (waitingInjections != null) {
					waitingForInjections [instance] = waitingInjections;

					waitingForInjections.Each (injection => {
						injectionsForInstances.Ensure (injection).Add (instance);
					});
				}

				InvokeMethods(instance, info.postInjections);
			}
		}

		InjectionInfo ParseInfo(Type type)
		{
			IDictionary<string, Type> injections = null;
			HashSet<Type> waitingFor = null;
			GetPropertyInjections(type.GetProperties(), ref injections, ref waitingFor);
			GetFieldInjections(type.GetFields(), ref injections, ref waitingFor);
			IList<MethodInfo> postInjections = GetPostMethods<PostInjection>(type.GetMethods());
			IList<MethodInfo> postInits = GetPostMethods<PostInit>(type.GetMethods());

			InjectionInfo info = null;

			if(injections != null || postInjections != null || postInits != null) {
				info = new InjectionInfo {
					injections = injections,
					postInjections = postInjections,
					postInits = postInits,
					waitingFor = waitingFor
				};
			}

			return info;
		}

		HashSet<object> InjectInstances<T>(T instance, Type type, IDictionary<string, Type> properties, BindingFlags flags, HashSet<Type> waitingFor)
		{
			HashSet<object> waitingInjections = null;

			if(properties != null) {
				waitingInjections = new HashSet<object> ();

				properties.Each(pair => {
					Type injectionType = pair.Value;
					object injection = GetInstance(pair.Value);
					object[] param = { injection };

					type.InvokeMember(pair.Key, flags, null, instance, param);

					if (waitingFor != null && waitingFor.Contains (injectionType)) {
						waitingInjections.Add (injection);
					}
				});
			}

			return waitingInjections;
		}

		void InvokeMethods(object instance, IList<MethodInfo> methods)
		{
			if(methods != null) {
				methods.Each(i => i.Invoke(instance, EMPTY_PARAMS));
			}
		}

		IList<MethodInfo> GetPostMethods<T>(MethodInfo[] methods) where T: Attribute
		{
			IList<MethodInfo> injections = null;

			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if(attribute is T) {
					injections = injections ?? new List<MethodInfo>();
					injections.Add(method);
				}
			}));

			return injections;
		}

		void GetFieldInjections(FieldInfo[] fields, ref IDictionary<string, Type> injections, ref HashSet<Type> waitingFor)
		{
			foreach (var field in fields) {
				ParseAttributes (field, field.FieldType, ref injections, ref waitingFor);
			}
		}

		void GetPropertyInjections(PropertyInfo[] properties, ref IDictionary<string, Type> injections, ref HashSet<Type> waitingFor)
		{
			foreach (var property in properties) {
				ParseAttributes (property, property.PropertyType, ref injections, ref waitingFor);
			}
		}

		void ParseAttributes(MemberInfo info, Type type, ref IDictionary<string, Type> injections, ref HashSet<Type> waitingFor)
		{
			object[] attributes = info.GetCustomAttributes(true);

			foreach(Inject attribute in attributes) {
				if(attribute != null) {
					injections = injections ?? new Dictionary<string, Type>();
					injections[info.Name] = type;

					if (attribute.hasToBeInitialized) {
						waitingFor = waitingFor ?? new HashSet<Type> ();
						waitingFor.Add (type);
					}
				}
			}
		}
	}

	public class InjectionInfo
	{
		public IList<MethodInfo> postInjections;
		public IList<MethodInfo> postInits;
		public IDictionary<string, Type> injections;
		public HashSet<Type> waitingFor;
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Inject : Attribute
	{
		public readonly bool hasToBeInitialized;

		public Inject (bool initialized = false)
		{
			hasToBeInitialized = initialized;
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInjection : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInit : Attribute
	{
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
