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

		public Injector (IContext cont)
		{
			context = cont;
		}

		public void Inject<T> (T instance)
		{
			var key = instance.GetType();
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
				var injection = context.GetInstance(pair.Value);
				var param = new object[]{ injection };

				type.InvokeMember(pair.Key, flags, null, instance, param);
			}
		}

		void InvokeMethods (object instance, IList<MethodInfo> methods, object[] param = null)
		{
			methods.Each(m => m.Invoke(instance, param ?? EMPTY_PARAMS));
		}

		void RegisterCleanups<T> (T instance, IList<MethodInfo> methods)
		{
			methods.Each(m => context.OnCleanUp.Add(() => m.Invoke(instance, EMPTY_PARAMS)));
		}

		public void Cleanup ()
		{
			infoMap.Clear();
		}
	}
}
