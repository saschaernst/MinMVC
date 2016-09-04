namespace MinMVC
{
	public interface IMediators
	{
		void Map<TMediated, TMediator>() where TMediated : IMediated where TMediator : IMediator;

		void Mediate<T>(T mediated) where T : IMediated;
	}
}
