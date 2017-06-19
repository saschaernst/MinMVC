using System;
using System.Collections.Generic;
using System.Reflection;
using MinTools;

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
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			fields.Each(f => ParseAttributes(f, f.FieldType, info));
		}

		void ParsePropertyAttributes (Type type, InjectionInfo info)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
			properties.Each(p => ParseAttributes(p, p.PropertyType, info));
		}

		void ParseAttributes (MemberInfo memberInfo, Type type, InjectionInfo info)
		{
			var customAttributes = memberInfo.GetCustomAttributes(true);

			foreach (var customAttribute in customAttributes) {
				if (customAttribute is Inject) {
					info.AddInjection(memberInfo.Name, type);
					break;
				}
			}
		}

		void ParseMethodAttributes<T> (Type type, InjectionInfo info) where T : Attribute
		{
			var methods = type.GetMethods();
			var taggedMethods = info.GetCalls<T>();

			foreach (var method in methods) {
				var attributes = method.GetCustomAttributes(true);

				foreach (var attribute in attributes) {
					if (attribute is T) {
						taggedMethods.Add(method);
					}
				}
			}
		}
	}

	public class InjectionInfo
	{
		readonly IDictionary<string, Type> injections = new Dictionary<string, Type>();
		readonly IDictionary<Type, List<MethodInfo>> calls = new Dictionary<Type, List<MethodInfo>>();

		public bool HasInjections ()
		{
			return injections.Count > 0;
		}

		public void AddInjection (string key, Type value)
		{
			injections[key] = value;
		}

		public IDictionary<string, Type> GetInjections ()
		{
			return injections;
		}

		public List<MethodInfo> GetCalls<T> () where T : Attribute
		{
			return calls.Retrieve(typeof(T));
		}

		public bool HasCalls<T> () where T : Attribute
		{
			return calls.ContainsKey(typeof(T));
		}
	}
}

