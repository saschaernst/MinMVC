using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	class Injector : IInjector
	{
		static readonly object[] EMPTY_PARAMS = new object [0];

		public Func<Type, object> GetInstance { private get; set; }

		readonly IDictionary<Type, InjectionInfo> _infoMap = new Dictionary<Type, InjectionInfo>();

		public void Inject<T>(T instance)
		{
			Type key = instance.GetType();
			InjectionInfo info;

			if(!_infoMap.TryGetValue(key, out info)) {
				_infoMap[key] = info = ParseInfo(key);
			}

			if(info != null) {
				InjectProperties(instance, key, info.propertyInjections, BindingFlags.SetProperty);
				InjectProperties(instance, key, info.fieldInjections, BindingFlags.SetField);
				InvokePostInjections(instance, info.postInjections);
			}
		}

		InjectionInfo ParseInfo(Type type)
		{
			IDictionary<string, Type> propertyInjections = GetPropertyInjections(type.GetProperties());
			IDictionary<string, Type> fieldInjections = GetFieldInjections(type.GetFields());
			IList<MethodInfo> postInjections = GetPostInjections(type.GetMethods());

			InjectionInfo info = null;

			if(propertyInjections != null || fieldInjections != null || postInjections != null) {
				info = new InjectionInfo {
					propertyInjections = propertyInjections,
					fieldInjections = fieldInjections,
					postInjections = postInjections
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

		void InvokePostInjections(object instance, IList<MethodInfo> injections)
		{
			if(injections != null) {
				injections.Each(i => i.Invoke(instance, EMPTY_PARAMS));
			}
		}

		IList<MethodInfo> GetPostInjections(MethodInfo[] methods)
		{
			IList<MethodInfo> injections = null;

			methods.Each(method => method.GetCustomAttributes(true).Each(attribute => {
				if(attribute is PostInjectionAttribute) {
					injections = injections ?? new List<MethodInfo>();
					injections.Add(method);
				}
			}));

			return injections;
		}

		IDictionary<string, Type> GetFieldInjections(FieldInfo[] fields)
		{
			IDictionary<string, Type> injections = null;
			fields.Each(field => ParseAttributes(field, field.FieldType, ref injections));

			return injections;
		}

		IDictionary<string, Type> GetPropertyInjections(PropertyInfo[] properties)
		{
			IDictionary<string, Type> injections = null;
			properties.Each(property => ParseAttributes(property, property.PropertyType, ref injections));

			return injections;
		}

		void ParseAttributes(MemberInfo info, Type type, ref IDictionary<string, Type> injections)
		{
			object[] attributes = info.GetCustomAttributes(true);

			foreach(InjectAttribute attribute in attributes) {
				if(attribute != null) {
					injections = injections ?? new Dictionary<string, Type>();
					injections[info.Name] = type;
				}
			}
		}
	}

	public class InjectionInfo
	{
		public IList<MethodInfo> postInjections;
		public IDictionary<string, Type> fieldInjections;
		public IDictionary<string, Type> propertyInjections;
	}

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class InjectAttribute : Attribute
	{
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostInjectionAttribute : Attribute
	{
	}
}
