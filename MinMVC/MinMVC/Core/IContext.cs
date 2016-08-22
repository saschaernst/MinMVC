using System;

namespace MinMVC
{
	public interface IContext
	{
		event Action OnCleanUp;

		IContext Parent { set; get; }

		void Register (Type type, bool preventCaching = false);

		void Register (Type key, Type value, bool preventCaching = false);

		void Register<T> (bool preventCaching = false);

		void Register<TInterface, TClass> (bool preventCaching = false);

		void RegisterInstance<T> (T instance, bool preventInjection = false);

		T Get<T> (Type key = null);

		object GetInstance (Type key);

		bool Has<T> ();

		bool Has (Type key);

		void Inject<T> (T instance);

		void CleanUp ();
	}
}
