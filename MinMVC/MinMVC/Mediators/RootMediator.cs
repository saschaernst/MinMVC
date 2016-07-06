namespace MinMVC
{
	public class RootMediator<T> : Mediator<T> where T : class, IRootView
	{
		[Inject]
		public IMediators mediators;

		protected override void Register ()
		{
			view.mediate = mediators.Mediate;
		}

		protected override void Unregister ()
		{
			view.mediate = null;
		}
	}
}
