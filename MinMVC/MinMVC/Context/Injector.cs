using System;
using System.Reflection;
using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	class Injector
	{
		static readonly object[] EMPTY_PARAMS = new object[0];

		readonly IDictionary<Type, InjectionInfo> infoMap = new Dictionary<Type, InjectionInfo>();
		readonly InfoParser parser = new InfoParser();

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
			foreach (var pair in injectionMap) {
				object injection = context.GetInstance(pair.Value);
				object[] param = { injection };

				type.InvokeMember(pair.Key, flags, null, instance, param);
			}
		}

		void InvokeMethods (object instance, IList<MethodInfo> methods, object[] param = null)
		{
			for (int i = 0; i < methods.Count; i++) {
				methods[i].Invoke(instance, param ?? EMPTY_PARAMS);
			}
		}

		void RegisterCleanups<T> (T instance, IList<MethodInfo> methods)
		{
			for (int i = 0; i < methods.Count; i++) {
				var method = methods[i];
				Action action = () => method.Invoke(instance, EMPTY_PARAMS);
				context.OnCleanUp.Add(action);
			}
		}

		public void Cleanup ()
		{
			infoMap.Clear();
		}
	}
}
