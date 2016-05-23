namespace MinMVC
{
	public interface IMediators
	{
		void Map<TViewInterface, TMediator>() where TViewInterface : IMediatedView where TMediator : IMediator;

		void Mediate<T>(T view) where T : IMediatedView;
	}
}
