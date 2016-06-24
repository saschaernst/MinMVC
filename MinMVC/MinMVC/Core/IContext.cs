﻿using System;

namespace MinMVC
{
	public interface IContext
	{
		event Action onCleanUp;

		IContext parent { set; }

		void Register<TInterface, TClass> (bool preventCaching = false);

		void Register (Type type, bool preventCaching = false);

		void Register (Type key, Type value, bool preventCaching = false);

		void Register<T> (bool preventCaching = false);

		void RegisterInstance<T> (T instance);

		T Get<T> (Type key = null) where T : class;

		object GetInstance (Type key);

		bool Has<T> ();

		bool Has (Type key);

		void Inject<T> (T instance);

		void CleanUp ();

		void OnStartInit (object instance);

		void OnInitDone (object instance);
	}
}
