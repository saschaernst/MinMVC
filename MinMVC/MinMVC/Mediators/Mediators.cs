using System;
using System.Collections.Generic;

namespace MinMVC
{
	public class Mediators : IMediators
	{
		[Inject]
		public IContext context;

		readonly IDictionary<Type, HashSet<Type>> _viewMediatorsMap = new Dictionary<Type, HashSet<Type>>();

		public void Map<TViewInterface, TMediator>() where TViewInterface : IMediatedView where TMediator : IMediator
		{
			Type viewType = typeof(TViewInterface);
			HashSet<Type> mediatorTypes = _viewMediatorsMap.Ensure (viewType);

			if(!context.Has<TMediator>()) {
				context.Register<TMediator>(true);
			}

			Type mediatorType = typeof(TMediator);
			mediatorTypes.Add(mediatorType);
		}

		public void Mediate<T>(T view) where T : IMediatedView
		{
			Type viewType = view.GetType();
			bool hasMediators = Create(view, viewType);

			viewType.GetInterfaces().Each(viewInterface => {
				hasMediators |= Create(view, viewInterface);
			});

			if(hasMediators) {
				view.OnMediation();
			} else {
				throw new NoMediatorMappingException("no mediators for " + viewType);
			}
		}

		bool Create<T>(T view, Type viewType) where T : IMediatedView
		{
			HashSet<Type> mediatorTypes;
			bool hasMediators = _viewMediatorsMap.TryGetValue(viewType, out mediatorTypes);

			if(hasMediators) {
				mediatorTypes.Each(mediatorType => context.Get<IMediator>(mediatorType).Init(view));
			}

			return hasMediators;
		}
	}

	public class NoMediatorMappingException : Exception
	{
		public NoMediatorMappingException(string message) : base(message)
		{
		}
	}
}
