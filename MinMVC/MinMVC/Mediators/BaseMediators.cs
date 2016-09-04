using System;
using System.Collections.Generic;

namespace MinMVC
{
	public abstract class BaseMediators : IMediators
	{
		readonly IDictionary<Type, HashSet<Type>> mediatingMap = new Dictionary<Type, HashSet<Type>>();

		public void Map<TMediated, TMediator> () where TMediated : IMediated where TMediator : IMediator
		{
			Type mediatedType = typeof(TMediated);
			HashSet<Type> mediatorTypes = mediatingMap.Retrieve(mediatedType);
			Type mediatorType = typeof(TMediator);
			mediatorTypes.Add(mediatorType);
		}

		public void Mediate<T> (T mediated) where T : IMediated
		{
			Type mediatedType = mediated.GetType();
			bool hasMediators = Create(mediated, mediatedType);

			mediatedType.GetInterfaces().Each(mediatedInterface => {
				hasMediators |= Create(mediated, mediatedInterface);
			});

			if (hasMediators) {
				mediated.OnMediation();
			}
			else {
				throw new NoMediatorMappingException("no mediators for " + mediatedType);
			}
		}

		bool Create<T> (T mediated, Type mediatedType) where T : IMediated
		{
			HashSet<Type> mediatorTypes;
			bool hasMediators = mediatingMap.TryGetValue(mediatedType, out mediatorTypes);

			if (hasMediators) {
				mediatorTypes.Each(mediatorType => Get(mediatorType).Init(mediated));
			}

			return hasMediators;
		}

		protected abstract IMediator Get (Type type);
	}

	public class NoMediatorMappingException : Exception
	{
		public NoMediatorMappingException (string message) : base(message)
		{
		}
	}
}
