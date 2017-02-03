using System.Collections.Generic;

namespace MinMVC
{
	//public interface ICachingCommand
	//{
	//	void Execute ();

	//	void OnExecute ();
	//}

	public class BaseCachingCommand<T> where T: new()
	{
		protected IList<T> cache;

		protected T Get ()
		{
			return cache.IsEmpty() ? new T() : cache.Pop();
		}
	}

	public abstract class CachingCommand<T> : BaseCachingCommand<T> where T : CachingCommand<T>, new()
	{
		public void Execute ()
		{
			if (cache == null) {
				OnExecute();
			}
			else {
				Get().OnExecute();
			}
		}

		public abstract void OnExecute ();
	}

	public abstract class CachingCommand<T, U> : BaseCachingCommand<T> where T : CachingCommand<T, U>, new()
	{
		public void Execute (U param)
		{
			if (cache == null) {
				OnExecute(param);
			}
			else {
				Get().OnExecute(param);
			}
		}

		public abstract void OnExecute (U param);
	}

	public class ExampleCommand : CachingCommand<ExampleCommand>
	{
		public override void OnExecute ()
		{
			
		}
	}

	public class ExampleParamCommand : CachingCommand<ExampleParamCommand, int>
	{
		public override void OnExecute (int param)
		{
			
		}
	}
}
