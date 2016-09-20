namespace MinMVC
{
	public class AutoMediator<T> : Mediator<T> where T : class, IMediating
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
