using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	public class Context : IContext
	{
		public event Action onCleanUp = delegate { };

		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();
		readonly HashSet<object> forceInjections = new HashSet<object>();

		IContext _parent;

		public IContext parent
		{
			get
			{
				return _parent;
			}

			set
			{
				if (hasParent) {
					_parent.onCleanUp -= CleanUp;
				}

				_parent = value;

				if (hasParent) {
					_parent.onCleanUp += CleanUp;
				}
			}
		}

		IContext root
		{
			get
			{
				IContext current = this;

				while (current.parent != null) {
					current = current.parent;
				}

				return current;
			}
		}

		public Context ()
		{
			RegisterInstance<IContext>(this);
		}

		public void CleanUp ()
		{
			parent = null;
			onCleanUp();
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
			if (!typeMap.ContainsKey(key)) {
				typeMap[key] = value;

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
			object cachedInstance;
			instanceCache.TryGetValue(key, out cachedInstance);

			if (cachedInstance == null) {
				instanceCache[key] = instance;
			}
			else {
				throw new AlreadyRegisteredException("already cached; " + key);
			}
		}

		public T Get<T> (Type key = null) where T : class
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
					instance = Activator.CreateInstance(value);
					Inject(instance);

					if (instanceCache.ContainsKey(key)) {
						instanceCache[key] = instance;
					}
				}
				else if (hasParent) {
					instance = _parent.GetInstance(key);
				}
			}
			else if (forceInjections.Contains(instance)) {
				Inject(instance);
			}

			if (instance == null) {
				throw new NotRegisteredException("not registered: " + key);
			}

			return instance;
		}

		public bool Has<T> ()
		{
			return Has(typeof(T));
		}

		public bool Has (Type key)
		{
			bool hasKey = instanceCache.ContainsKey(key) || typeMap.ContainsKey(key);
			bool findInParent = !hasKey && hasParent;

			return findInParent ? _parent.Has(key) : hasKey;
		}

		bool hasParent
		{
			get { return _parent != null; }
		}

		public void Inject<T> (T instance)
		{
			Type key = instance.GetType();
			InjectionInfo info = infoMap.Retrieve(key, () => ParseInfo(key));

			if (info != null) {
				InjectInstances(instance, key, info.injections, BindingFlags.SetProperty | BindingFlags.SetField);
				InvokeMethods(instance, info.postInjectionCalls);
				forceInjections.Remove(instance);
			}
		}

		InjectionInfo ParseInfo (Type type)
		{
			InjectionInfo info = new InjectionInfo();

			ParsePropertyAttributes(type.GetProperties(), info);
			ParseFieldAttributes(type.GetFields(), info);
			ParsePostMethods<PostInjection>(type.GetMethods(), info.postInjectionCalls);

			return info;
		}

		void InjectInstances<T> (T instance, Type type, IDictionary<string, Type> injectionMap, BindingFlags flags)
		{
			injectionMap.Each(pair => {
				Type injectionType = pair.Value;
				object injection = GetInstance(pair.Value);
				object[] param = { injection };

				type.InvokeMember(pair.Key, flags, null, instance, param);
			});
		}

		void InvokeMethods (object instance, IEnumerable<MethodInfo> methods)
		{
			methods.Each(method => method.Invoke(instance, EMPTY_PARAMS));
		}

		void ParseFieldAttributes (FieldInfo[] fields, InjectionInfo info)
		{
			fields.Each(field => ParseAttributes(field, field.FieldType, info.injections));
		}

		void ParsePropertyAttributes (PropertyInfo[] properties, InjectionInfo info)
		{
			properties.Each(property => ParseAttributes(property, property.PropertyType, info.injections));
		}

		void ParsePostMethods<T> (IEnumerable<MethodInfo> methods, HashSet<MethodInfo> postMethods) where T : Attribute
		{
			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if (attribute is T) {
					postMethods.Add(method);
				}
			}));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, IDictionary<string, Type> injections)
		{
			memberInfo.GetCustomAttributes(true).Each(attribute => {
				if (attribute is Inject) {
					injections[memberInfo.Name] = type;
				}
			});
		}
	}

	public class InjectionInfo
	{
		public HashSet<MethodInfo> postInjectionCalls = new HashSet<MethodInfo>();
		public IDictionary<string, Type> injections = new Dictionary<string, Type>();
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Inject : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInjection : Attribute
	{

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
