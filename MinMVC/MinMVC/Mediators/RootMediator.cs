namespace MinMVC
{
	public class RootMediator<T> : Mediator<T> where T : class, IMediatedRoot
	{
		[Inject]
		public IMediators mediators;

		protected override void Register ()
		{
			mediated.Mediate = mediators.Mediate;
		}

		protected override void Unregister ()
		{
			mediated.Mediate = null;
		}
	}
}
