namespace MinMVC
{
	public abstract class Mediator<T> : IMediator where T : class, IMediated
	{
		protected T mediated;

		public void Init (IMediated med)
		{
			mediated = (T)med;
			mediated.OnRemove += Remove;

			Register();
		}

		protected abstract void Register ();

		protected void Remove ()
		{
			Unregister();

			mediated.OnRemove -= Remove;
			mediated = null;
		}

		protected abstract void Unregister ();
	}
}
