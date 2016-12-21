using System;
using System.Collections.Generic;
using System.Reflection;

namespace MinMVC
{
	class InfoParser
	{
		public InjectionInfo Parse (Type type)
		{
			var info = new InjectionInfo();
			ParsePropertyAttributes(type, info);
			ParseFieldAttributes(type, info);
			ParseMethodAttributes<PostInjection>(type, info);
			ParseMethodAttributes<Cleanup>(type, info);

			return info;
		}

		void ParseFieldAttributes (Type type, InjectionInfo info)
		{
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			for (int i = 0; i < fields.Length; i++) {
				var field = fields[i];
				ParseAttributes(field, field.FieldType, info);
			}
		}

		void ParsePropertyAttributes (Type type, InjectionInfo info)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy);

			for (int i = 0; i < properties.Length; i++) {
				var property = properties[i];
				ParseAttributes(property, property.PropertyType, info);
			}
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, InjectionInfo info)
		{
			var customAttributes = memberInfo.GetCustomAttributes(true);

			for (int i = 0; i < customAttributes.Length; i++) {
				if (customAttributes[i] is Inject) {
					info.AddInjection(memberInfo.Name, type);
				}
			}
		}

		void ParseMethodAttributes<T> (Type type, InjectionInfo info) where T : Attribute
		{
			var methods = type.GetMethods();
			IList<MethodInfo> taggedMethods = null;

			for (int i = 0; i < methods.Length; i++) {
				var method = methods[i];
				var attributes = method.GetCustomAttributes(true);

				for (int j = 0; j < attributes.Length; j++) {
					if (attributes[j] is T) {
						taggedMethods = taggedMethods ?? info.GetCalls<T>();
						taggedMethods.Add(method);
					}
				}
			}
		}
	}

	public class InjectionInfo
	{
		IDictionary<string, Type> injections;
		IDictionary<Type, IList<MethodInfo>> calls;

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

		public IList<MethodInfo> GetCalls<T> () where T : Attribute
		{
			calls = calls ?? new Dictionary<Type, IList<MethodInfo>>();

			return calls.Retrieve(typeof(T), CreateList);
		}

		IList<MethodInfo> CreateList ()
		{
			return new List<MethodInfo>();
		}

		public bool HasCalls<T> () where T : Attribute
		{
			return calls != null && calls.ContainsKey(typeof(T));
		}
	}
}

