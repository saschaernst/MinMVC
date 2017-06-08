using System;
using System.Collections.Generic;
using MinTools;

namespace MinMVC
{
	public abstract class BaseMediators : IMediators
	{
		readonly IDictionary<Type, List<Type>> mediatingMap = new Dictionary<Type, List<Type>>();

		public void Map<TMediated, TMediator> () where TMediated : IMediated where TMediator : IMediator
		{
			var mediatorTypes = mediatingMap.Retrieve(typeof(TMediated));
			mediatorTypes.Add(typeof(TMediator));
		}

		public void Mediate (IMediated mediated)
		{
			var mediatedType = mediated.GetType();
			var hasMediators = Create(mediated, mediatedType);
			var mediatedInterfaces = mediatedType.GetInterfaces();
			mediatedInterfaces.Each(i => hasMediators |= Create(mediated,i));

			if (hasMediators) {
				mediated.OnMediation();
			}
			else {
				throw new NoMediatorMappingException("no mediators for " + mediatedType);
			}
		}

		bool Create<T> (T mediated, Type mediatedType) where T : IMediated
		{
			var mediatorTypes = mediatingMap.Retrieve(mediatedType);
			mediatorTypes.Each(t => Get(t).Init(mediated));

			return mediatorTypes.Count > 0;
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
