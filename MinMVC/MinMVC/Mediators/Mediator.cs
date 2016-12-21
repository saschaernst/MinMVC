namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediated
	{
		protected T mediated;

		public void Init (IMediated med)
		{
			mediated = (T)med;
			mediated.OnStart += OnStart;
			mediated.OnRemove += OnRemove;

			Register();
		}

		protected abstract void Register ();

		protected virtual void OnStart ()
		{
			
		}

		protected void OnRemove ()
		{
			Unregister();

			mediated.OnRemove -= OnRemove;
			mediated = null;
		}

		protected abstract void Unregister ();
	}
}
