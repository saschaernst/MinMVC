using System;

namespace MinMVC
{
	public class Mediators : BaseMediators
	{
		[Inject]
		public IContext context;

		protected override IMediator Get (Type type)
		{
			return context.Get<IMediator>(type);
		}
	}
}
