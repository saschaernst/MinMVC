﻿using System;

namespace MinMVC
{
	public interface IContext
	{
		event Action OnCleanUp;

		string Id { get; }

		IContext Parent { set; get; }

		void Register (Type type, bool preventCaching = false);

		void Register (Type key, Type value, bool preventCaching = false);

		void Register<T> (bool preventCaching = false) where T : class, new();

		void Register<TInterface, TClass> (bool preventCaching = false) where TInterface : class where TClass : class, new();

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
