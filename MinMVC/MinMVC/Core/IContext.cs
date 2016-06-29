using System;

namespace MinMVC
{
	public interface IContext
	{
		event Action onCleanUp;

		IContext parent { set; get; }

		void Register<TInterface, TClass> (bool preventCaching = false);

		void Register (Type type, bool preventCaching = false);

		void Register (Type key, Type value, bool preventCaching = false);

		void Register<T> (bool preventCaching = false);

		void RegisterInstance<T> (T instance, bool forceInjection = false);

		T Get<T> (Type key = null) where T : class;

		object GetInstance (Type key);

		bool Has<T> ();

		bool Has (Type key);

		void Inject<T> (T instance);

		void CleanUp ();
	}
}
