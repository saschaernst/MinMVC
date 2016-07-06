namespace MinMVC
{
	public interface ICommands
	{
		ICommandCache Get<T> (bool init = false) where T : class, IBaseCommand, new();

		bool Has<T> () where T : class, IBaseCommand, new();

		void Remove<T> () where T : class, IBaseCommand, new();
	}
}
