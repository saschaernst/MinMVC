namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediatedView
	{
		protected T _view;

		public void Init(IMediatedView view)
		{
			_view = (T)view;
			_view.onRemove += Remove;

			Register();
		}

		protected abstract void Register();

		protected void Remove()
		{
			Unregister();

			_view.onRemove -= Remove;
			_view = null;
		}

		protected abstract void Unregister();
	}
}
