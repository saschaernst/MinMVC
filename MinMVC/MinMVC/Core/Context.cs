using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	public class Context : IContext
	{
		static IContext _root;

		public static IContext root {
			get {
				return _root = _root ?? new Context();
			}
		}

		public event Action onCleanUp = delegate { };

		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();
		readonly HashSet<object> forceInjections = new HashSet<object>();

		IContext _parent;

		public IContext parent {
			get {
				return _parent;
			}

			set {
				if (hasParent) {
					_parent.onCleanUp -= CleanUp;
				}

				_parent = value;

				if (hasParent) {
					_parent.onCleanUp += CleanUp;
				}
			}
		}

		bool hasParent {
			get { return _parent != null; }
		}

		public Context (IContext p = null)
		{
			RegisterInstance<IContext>(this);
			parent = p;
		}

		public void CleanUp ()
		{
			parent = null;

			onCleanUp();
			onCleanUp = null;

			infoMap.Clear();
			typeMap.Clear();
			instanceCache.Clear();
			forceInjections.Clear();

			if (_root == this) {
				_root = null;
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
					instance = Activator.CreateInstance(value);
					instanceCache.UpdateEntry(key, instance);
					Inject(instance);
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
			bool hasKey = typeMap.ContainsKey(key);
			bool findInParent = !hasKey && hasParent;

			return findInParent ? _parent.Has(key) : hasKey;
		}

		public void Inject<T> (T instance)
		{
			Type key = instance.GetType();
			var info = infoMap.Retrieve(key, () => ParseInfo(key));
			InjectInstances(instance, key, info.injections, BindingFlags.SetProperty | BindingFlags.SetField);
			InvokeMethods(instance, info.postInjectionCalls);
			RegisterCleanups(instance, info.cleanupCalls);

			forceInjections.Remove(instance);
		}

		InjectionInfo ParseInfo (Type type)
		{
			var info = new InjectionInfo();
			ParsePropertyAttributes(type.GetProperties(), info.injections);
			ParseFieldAttributes(type.GetFields(), info.injections);
			ParseMethods<PostInjection>(type.GetMethods(), info.postInjectionCalls);
			ParseMethods<Cleanup>(type.GetMethods(), info.cleanupCalls);

			return info;
		}

		void InjectInstances<T> (T instance, Type type, IDictionary<string, Type> injectionMap, BindingFlags flags)
		{
			injectionMap.Each(pair => {
				object injection = GetInstance(pair.Value);
				object[] param = { injection };

				type.InvokeMember(pair.Key, flags, null, instance, param);
			});
		}

		void InvokeMethods (object instance, IEnumerable<MethodInfo> methods, object[] param = null)
		{
			methods.Each(method => method.Invoke(instance, param ?? EMPTY_PARAMS));
		}

		void RegisterCleanups<T> (T instance, HashSet<MethodInfo> methods)
		{
			methods.Each(method => onCleanUp += () => method.Invoke(instance, EMPTY_PARAMS));
		}

		void ParseFieldAttributes (FieldInfo[] fields, IDictionary<string, Type> result)
		{
			fields.Each(field => ParseAttributes(field, field.FieldType, result));
		}

		void ParsePropertyAttributes (PropertyInfo[] properties, IDictionary<string, Type> result)
		{
			properties.Each(property => ParseAttributes(property, property.PropertyType, result));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, IDictionary<string, Type> result)
		{
			memberInfo.GetCustomAttributes(true).Each(attribute => {
				if (attribute is Inject) {
					result[memberInfo.Name] = type;
				}
			});
		}

		void ParseMethods<T> (IEnumerable<MethodInfo> methods, HashSet<MethodInfo> postMethods) where T : Attribute
		{
			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if (attribute is T) {
					postMethods.Add(method);
				}
			}));
		}
	}

	public class InjectionInfo
	{
		public IDictionary<string, Type> injections = new Dictionary<string, Type>();
		public HashSet<MethodInfo> postInjectionCalls = new HashSet<MethodInfo>();
		public HashSet<MethodInfo> cleanupCalls = new HashSet<MethodInfo>();
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Inject : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInjection : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class Cleanup : Attribute
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
