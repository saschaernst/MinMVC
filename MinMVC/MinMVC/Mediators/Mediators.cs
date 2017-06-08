using System;

namespace MinMVC
{
	public class Mediators : BaseMediators
	{
		[Inject] public IContext context;

		protected override IMediator Get (Type type)
		{
			if (!type.IsInterface && !context.Has(type)) {
				context.RegisterType(type, true);
			}

			return context.Get<IMediator>(type);
		}
	}
}
