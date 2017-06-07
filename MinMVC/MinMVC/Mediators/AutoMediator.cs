namespace MinMVC
{
	public class AutoMediator<T> : Mediator<T> where T : class, IMediating
	{
		[Inject]
		public IMediators mediators;

		protected override void Register ()
		{
			mediated.SetMediateHandler(mediators.Mediate);
		}
	}
}
