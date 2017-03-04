namespace MinMVC
{
	public interface IMediators
	{
		void Map<TMediated, TMediator>() where TMediated : IMediated where TMediator : IMediator;

		void Mediate(IMediated mediated);
	}
}
