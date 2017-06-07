using System;
using MinTools;

namespace MinMVC
{
	public interface IContext
	{
		MinSignal OnCleanUp { get;  }

		string Id { get; }

		IContext Parent { set; get; }

		void RegisterType (Type type, bool preventCaching = false);

		void RegisterType (Type key, Type value, bool preventCaching = false);

		void RegisterClass<T> (bool preventCaching = false) where T : class, new();

		void RegisterClass<TInterface, TClass> (bool preventCaching = false) where TInterface : class where TClass : class, new();

		void RegisterInstance<T> (T instance, bool preventInjection = false);

		void RegisterHandler<T>(Func<object> handler);

		void RegisterHandler(Type key, Func<object> handler);

		T Get<T> (Type key = null);

		object GetInstance (Type key);

		bool Has<T> ();

		bool Has (Type key);

		void Inject<T> (T instance);

		T Create<T>() where T: new();

		void CleanUp ();
	}
}
