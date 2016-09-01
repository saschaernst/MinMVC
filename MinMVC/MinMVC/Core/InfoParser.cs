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
			type.GetFields().For(field => ParseAttributes(field, field.FieldType, info));
		}

		void ParsePropertyAttributes (Type type, InjectionInfo info)
		{
			type.GetProperties().For(property => ParseAttributes(property, property.PropertyType, info));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, InjectionInfo info)
		{
			memberInfo.GetCustomAttributes(true).For(attribute => {
				if (attribute is Inject) {
					info.AddInjection(memberInfo.Name, type);
				}
			});
		}

		void ParseMethodAttributes<T> (Type type, InjectionInfo info) where T : Attribute
		{
			var methods = type.GetMethods();
			HashSet<MethodInfo> taggedMethods = null;

			methods.For(method => method.GetCustomAttributes(true).For(attribute => {
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
}

