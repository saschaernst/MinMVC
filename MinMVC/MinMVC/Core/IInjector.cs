using System;

namespace MinMVC
{
	public interface IInjector
	{
		Func<Type, object> GetInstance { set; }

		void Inject<T>(T instance);
	}
}
