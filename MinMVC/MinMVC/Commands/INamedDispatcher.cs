namespace MinMVC
{
	public interface INamedDispatcher
	{
		void Register<T> (string eventName) where T : class, IBaseCommand, new();

		void Unregister<T> (string eventName) where T : class, IBaseCommand, new();

		void UnregisterAll (string eventName);

		void Execute (string eventName);

		void Execute<TParam> (string eventName, TParam param);

		void Execute<TParam0, TParam1> (string eventName, TParam0 param0, TParam1 param1);
	}
}
