namespace MinMVC
{
	public class Commands : BaseCommands
	{
		[Inject]
		public IContext context;

		[Cleanup]
		protected void CleanUp ()
		{
			ClearCaches();
		}

		public override IBaseCommand GetCommand<T> ()
		{
			if (!context.Has<T>()) {
				context.Register<T>(true);
			}

			return context.Get<T>();
		}
	}
}
