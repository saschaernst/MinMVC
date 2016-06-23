using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	class Injector : IInjector
	{
		static readonly object[] EMPTY_PARAMS = new object [0];

		public Func<Type, object> GetInstance { private get; set; }

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly IDictionary<object, HashSet<Type>> waitingList = new Dictionary<object, HashSet<Type>> ();

		public void Inject<T>(T instance)
		{
			Type key = instance.GetType();
			InjectionInfo info;

			if(!infoMap.TryGetValue(key, out info)) {
				infoMap[key] = info = ParseInfo(key);
			}

			if(info != null) {
				InjectProperties(instance, key, info.injections, BindingFlags.SetProperty | BindingFlags.SetField);
				InvokeMethods(instance, info.postInjections);

				if (info.waitingFor != null) {
					waitingList [instance] = info.waitingFor;
				}
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

		void InjectProperties<T>(T instance, Type type, IDictionary<string, Type> properties, BindingFlags flags)
		{
			if(properties != null) {
				properties.Each(pair => {
					object injection = GetInstance(pair.Value);
					object[] param = { injection };

					type.InvokeMember(pair.Key, flags, null, instance, param);
				});
			}
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
}
