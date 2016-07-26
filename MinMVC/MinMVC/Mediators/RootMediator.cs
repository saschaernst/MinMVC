namespace MinMVC
{
	public class RootMediator<T> : Mediator<T> where T : class, IRootView
	{
		[Inject]
		public IMediators mediators;

		protected override void Register ()
		{
			view.Mediate = mediators.Mediate;
		}

		protected override void Unregister ()
		{
			view.Mediate = null;
		}
	}
}
