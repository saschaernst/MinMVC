﻿using System;
using System.Reflection;
using System.Collections.Generic;

namespace MinMVC
{
	class Injector
	{
		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly InfoParser parser = new InfoParser ();

		IContext context;

		public Injector (IContext context)
		{
			this.context = context;
		}
		
		public void Inject<T> (T instance)
		{
			Type key = instance.GetType();
			var info = infoMap.Retrieve(key, () => parser.Parse(key));

			if (info.HasInjections()) {
				InjectInstances(instance, key, info.GetInjections(), BindingFlags.SetProperty | BindingFlags.SetField);
			}

			if (info.HasCalls<PostInjection>()) {
				InvokeMethods(instance, info.GetCalls<PostInjection>());
			}

			if (info.HasCalls<Cleanup>()) {
				RegisterCleanups(instance, info.GetCalls<Cleanup>());
			}
		}

		void InjectInstances<T> (T instance, Type type, IDictionary<string, Type> injectionMap, BindingFlags flags)
		{
			injectionMap.Each(pair => {
				object injection = context.GetInstance(pair.Value);
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
			methods.Each(method => context.OnCleanUp += () => method.Invoke(instance, EMPTY_PARAMS));
		}

		public void Cleanup ()
		{
			infoMap.Clear ();
		}
	}
}
