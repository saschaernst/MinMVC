namespace MinMVC
{
	public class AutoMediator<T> : Mediator<T> where T : class, IMediating
	{
		[Inject]
		public IMediators mediators;

		protected override void Register ()
		{
			mediated.OnMediate = mediators.Mediate;
			mediated.ResolveQueue();
		}

		protected override void Unregister ()
		{
			mediated.OnMediate = null;
		}
	}
}
