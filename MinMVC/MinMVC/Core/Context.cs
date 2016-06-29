using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	public class Context : IContext
	{
		public event Action onCleanUp = delegate { };
		public event Action<object> onCheckWaitingList = delegate { };

		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<Type, Type> typeMap = new Dictionary<Type, Type>();
		readonly IDictionary<Type, object> instanceCache = new Dictionary<Type, object>();
		readonly HashSet<object> forceInjections = new HashSet<object>();

		#region async init

		readonly HashSet<object> initializingInstances = new HashSet<object>();
		readonly IDictionary<object, HashSet<object>> instances2Injections = new Dictionary<object, HashSet<object>>();
		readonly IDictionary<object, HashSet<object>> injections2Instances = new Dictionary<object, HashSet<object>>();
		readonly IDictionary<object, HashSet<MethodInfo>> instances2PostInits = new Dictionary<object, HashSet<MethodInfo>>();

		#endregion

		IContext _parent;

		public IContext parent {
			get {
				return _parent;
			}

			set {
				if (hasParent) {
					_parent.onCheckWaitingList -= CheckWaitingList;
					_parent.onCleanUp -= CleanUp;
				}

				_parent = value;

				if (hasParent) {
					_parent.onCheckWaitingList += CheckWaitingList;
					_parent.onCleanUp += CleanUp;
				}
			}
		}

		IContext root {
			get {
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
			} else {
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
			} else {
				throw new AlreadyRegisteredException("already cached; " + key);
			}
		}

		#region async init

		public void InitDone (object initializedInstance)
		{
			if (!initializingInstances.Remove(initializedInstance)) {
				throw new InitializingException(initializedInstance + " is not initializing");
			}

			CheckWaitingList(initializedInstance);
		}

		void CheckWaitingList (object initializedInstance)
		{
			if (injections2Instances.ContainsKey(initializedInstance)) {
				var instances = injections2Instances.Withdraw(initializedInstance);

				instances.Each(instance => {
					var injections = instances2Injections[instance];
					injections.Remove(initializedInstance);

					if (injections.IsEmpty()) {
						instances2Injections.Remove(instance);
						var postInits = instances2PostInits.Withdraw(instance);
						InvokePostInits(instance, postInits);
					}
				});
			}

			onCheckWaitingList(initializedInstance);
		}

		void InvokePostInits (object instance, HashSet<MethodInfo> postInits)
		{
			InvokeMethods(instance, postInits);
			InitDone(instance);
		}

		public bool IsInitializing (object injection)
		{
			bool isInitializing = initializingInstances.Contains(injection);

			return !isInitializing && hasParent ? _parent.IsInitializing(injection) : isInitializing;
		}

		#endregion

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
				} else if (hasParent) {
					instance = _parent.GetInstance(key);
				}
			} else if (forceInjections.Contains(instance)) {
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

		bool hasParent {
			get { return _parent != null; }
		}

		public void Inject<T> (T instance)
		{
			Type key = instance.GetType();
			InjectionInfo info = infoMap.Retrieve(key, () => ParseInfo(key));

			if (info != null) {
				var waitingInjections = InjectInstances(instance, key, info.injections, BindingFlags.SetProperty | BindingFlags.SetField, info.waitingFor);

				if (waitingInjections != null) {
					instances2Injections[instance] = waitingInjections;
					instances2PostInits[instance] = info.postInits;
					waitingInjections.Each(injection => injections2Instances.Retrieve(injection).Add(instance));
				}

				if (info.waitForInit || info.postInits.Count > 0) {
					initializingInstances.Add(instance);
				}

				InvokeMethods(instance, info.postInjections);

				if (waitingInjections == null) {
					InvokePostInits(instance, info.postInits);
				}

				forceInjections.Remove(instance);
			}
		}

		InjectionInfo ParseInfo (Type type)
		{
			var info = new InjectionInfo();

			ParseClassAttributes(type.GetCustomAttributes(true), info);
			ParsePropertyAttributes(type.GetProperties(), info);
			ParseFieldAttributes(type.GetFields(), info);
			ParsePostMethods<PostInjection>(type.GetMethods(), info.postInjections);
			ParsePostMethods<PostInit>(type.GetMethods(), info.postInits);

			return info;
		}

		HashSet<object> InjectInstances<T> (T instance, Type type, IDictionary<string, Type> injectionMap, BindingFlags flags, ICollection<Type> waitingFor)
		{
			HashSet<object> waitingInjections = null;

			injectionMap.Each(pair => {
				Type injectionType = pair.Value;
				object injection = GetInstance(pair.Value);
				object[] param = { injection };

				type.InvokeMember(pair.Key, flags, null, instance, param);

				if (waitingFor != null && waitingFor.Contains(injectionType) && IsInitializing(injection)) {
					waitingInjections = waitingInjections ?? new HashSet<object>();
					waitingInjections.Add(injection);
				}
			});

			return waitingInjections;
		}

		void InvokeMethods (object instance, IEnumerable<MethodInfo> methods)
		{
			methods.Each(method => method.Invoke(instance, EMPTY_PARAMS));
		}

		void ParseClassAttributes (object[] attributes, InjectionInfo info)
		{
			foreach (Attribute attribute in attributes) {
				info.waitForInit |= attribute is WaitForInit;
			}
		}

		void ParseFieldAttributes (FieldInfo[] fields, InjectionInfo info)
		{
			fields.Each(field => ParseAttributes(field, field.FieldType, info));
		}

		void ParsePropertyAttributes (PropertyInfo[] properties, InjectionInfo info)
		{
			properties.Each(property => ParseAttributes(property, property.PropertyType, info));
		}

		void ParsePostMethods<T> (IEnumerable<MethodInfo> methods, HashSet<MethodInfo> postMethods) where T : Attribute
		{
			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if (attribute is T) {
					postMethods.Add(method);
				}
			}));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, InjectionInfo info)
		{
			var attributes = memberInfo.GetCustomAttributes(true);

			attributes.Each(attribute => {
				if (attribute is Inject) {
					info.injections[memberInfo.Name] = type;
					var injectionInfo = infoMap.Retrieve(type, () => ParseInfo(type));

					if (!(attribute is ForceInject) && injectionInfo.waitForInit) {
						info.waitingFor.Add(type);
					}
				}
			});
		}
	}

	public class InjectionInfo
	{
		public HashSet<MethodInfo> postInjections = new HashSet<MethodInfo>();
		public HashSet<MethodInfo> postInits = new HashSet<MethodInfo>();
		public IDictionary<string, Type> injections = new Dictionary<string, Type>();
		public HashSet<Type> waitingFor = new HashSet<Type>();
		public bool waitForInit;
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class WaitForInit : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class Inject : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class ForceInject : Inject
	{

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

	public class InitializingException : Exception
	{
		public InitializingException (string message) : base(message)
		{
		}
	}
}
