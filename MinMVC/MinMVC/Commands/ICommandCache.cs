namespace MinMVC
{
	public interface ICommandCache
	{
		void Execute ();

		void Execute<TParam> (TParam param);

		void Execute<TParam0, TParam1> (TParam0 param0, TParam1 param1);

		void Retain (IBaseCommand command);

		void Finish (IBaseCommand command);

		void CleanUp ();
	}
}
