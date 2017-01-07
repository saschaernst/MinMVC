using System;

namespace MinMVC
{
	public abstract class BaseCommand : IBaseCommand
	{
		public ICommandCache cache { private get; set; }

		public bool isRetained { get; private set; }

		protected void Retain ()
		{
			isRetained = true;
			cache.Retain(this);
		}

		protected virtual void Release ()
		{
			Cancel();
			isRetained = false;
			cache.Finish(this);
		}

		public virtual void Cancel ()
		{

		}
	}

	public abstract class Command : BaseCommand, ICommand
	{
		public abstract void Execute ();
	}

	public abstract class Command<TParam> : BaseCommand, ICommand<TParam>
	{
		public virtual void Execute (TParam param)
		{
			throw new NotImplementedException();
		}

		public virtual TParam Request ()
		{
			throw new NotImplementedException();
		}
	}

	public abstract class Command<TParam0, TParam1> : BaseCommand, ICommand<TParam0, TParam1>
	{
		public virtual void Execute (TParam0 param0, TParam1 param1)
		{
			throw new NotImplementedException();
		}

		public TParam0 Request (TParam1 param1)
		{
			throw new NotImplementedException();
		}
	}
}
