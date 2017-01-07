namespace MinMVC
{
	public interface ICommandCache
	{
		void Execute ();

		void Execute<TParam> (TParam param);

		void Execute<TParam0, TParam1> (TParam0 param0, TParam1 param1);

		TParam Request<TParam> ();

		TParam0 Request<TParam0, TParam1> (TParam1 param1);

		void Retain (IBaseCommand command);

		void Finish (IBaseCommand command);

		void CleanUp ();
	}
}
