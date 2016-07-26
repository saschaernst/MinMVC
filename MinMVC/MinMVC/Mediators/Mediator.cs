namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediatedView
	{
		protected T view;

		public void Init (IMediatedView v)
		{
			view = (T)v;
			view.OnRemove += Remove;

			Register();
		}

		protected abstract void Register ();

		protected void Remove ()
		{
			Unregister();

			view.OnRemove -= Remove;
			view = null;
		}

		protected abstract void Unregister ();
	}
}
