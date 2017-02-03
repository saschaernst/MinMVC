namespace MinMVC
{
	public interface ICommands
	{
		CommandCache<T> Get<T> (bool init = false) where T : class, IBaseCommand, new();

		bool Has<T> () where T : class, IBaseCommand, new();

		void Remove<T> () where T : class, IBaseCommand, new();

		IBaseCommand GetCommand<T> () where T : class, IBaseCommand, new();
	}
}
