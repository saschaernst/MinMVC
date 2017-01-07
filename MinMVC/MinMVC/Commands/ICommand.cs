namespace MinMVC
{
	public interface IBaseCommand
	{
		ICommandCache cache { set; }

		bool isRetained { get; }

		void Cancel ();
	}

	public interface ICommand : IBaseCommand
	{
		void Execute ();
	}

	public interface ICommand<T> : IBaseCommand
	{
		void Execute (T param);
		T Request ();
	}

	public interface ICommand<TParam0, TParam1> : IBaseCommand
	{
		void Execute (TParam0 param0, TParam1 param1);
		TParam0 Request (TParam1 param1);
	}
}
