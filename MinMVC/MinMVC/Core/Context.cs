using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	public class Context : IContext
	{
		public const string ROOT = "root";

		public static IContext root {
			get {
				return Get(ROOT);
			}
		}

		readonly static IDictionary<string, IContext> contexts = new Dictionary<string, IContext>();

		public static IContext Get (string id)
		{
			return contexts[id];
		}

		public static IContext Add (string id, IContext context = null)
		{
			return contexts[id] = context ?? new Context(id);
		}

		public event Action onCleanUp = delegate { };

		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();
		readonly HashSet<object> forceInjections = new HashSet<object>();

		public string id { get; private set; }

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

		public Context (string id = ROOT, IContext p = null)
		{
			this.id = id;
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

			contexts.Remove(id);
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

			if (info.HasInjections()) {
				InjectInstances(instance, key, info.GetInjections(), BindingFlags.SetProperty | BindingFlags.SetField);
			}

			if (info.HasCalls<PostInjection>()) {
				InvokeMethods(instance, info.GetCalls<PostInjection>());
			}

			if (info.HasCalls<Cleanup>()) {
				RegisterCleanups(instance, info.GetCalls<Cleanup>());
			}

			forceInjections.Remove(instance);
		}

		InjectionInfo ParseInfo (Type type)
		{
			var info = new InjectionInfo();
			ParsePropertyAttributes(type, info);
			ParseFieldAttributes(type, info);
			ParseMethodAttributes<PostInjection>(type, info);
			ParseMethodAttributes<Cleanup>(type, info);

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

		void ParseFieldAttributes (Type type, InjectionInfo info)
		{
			type.GetFields().Each(field => ParseAttributes(field, field.FieldType, info));
		}

		void ParsePropertyAttributes (Type type, InjectionInfo info)
		{
			type.GetProperties().Each(property => ParseAttributes(property, property.PropertyType, info));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, InjectionInfo info)
		{
			memberInfo.GetCustomAttributes(true).Each(attribute => {
				if (attribute is Inject) {
					info.AddInjection(memberInfo.Name, type);
				}
			});
		}

		void ParseMethodAttributes<T> (Type type, InjectionInfo info) where T : Attribute
		{
			var methods = type.GetMethods();
			HashSet<MethodInfo> taggedMethods = null;

			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if (attribute is T) {
					taggedMethods = taggedMethods ?? info.GetCalls<T>();
					taggedMethods.Add(method);
				}
			}));
		}
	}

	public class InjectionInfo
	{
		IDictionary<string, Type> injections;
		IDictionary<Type, HashSet<MethodInfo>> calls;

		public bool HasInjections ()
		{
			return injections != null;
		}

		public void AddInjection (string key, Type value)
		{
			injections = injections ?? new Dictionary<string, Type>();
			injections[key] = value;
		}

		public IDictionary<string, Type> GetInjections ()
		{
			return injections;
		}

		public HashSet<MethodInfo> GetCalls<T> () where T : Attribute
		{
			calls = calls ?? new Dictionary<Type, HashSet<MethodInfo>>();

			return calls.Retrieve(typeof(T));
		}

		public bool HasCalls<T> () where T : Attribute
		{
			return calls != null && calls.ContainsKey(typeof(T));
		}
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
