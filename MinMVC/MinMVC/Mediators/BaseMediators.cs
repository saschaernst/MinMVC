using System;
using System.Collections.Generic;

namespace MinMVC
{
	public abstract class BaseMediators : IMediators
	{
		readonly IDictionary<Type, IList<Type>> mediatingMap = new Dictionary<Type, IList<Type>>();

		public void Map<TMediated, TMediator> () where TMediated : IMediated where TMediator : IMediator
		{
			var mediatedType = typeof(TMediated);
			IList<Type> mediatorTypes;

			if(!mediatingMap.TryGetValue(mediatedType, out mediatorTypes))
			{
				mediatingMap[mediatedType] = mediatorTypes = new List<Type>();
			}

			var mediatorType = typeof(TMediator);
			mediatorTypes.Add(mediatorType);
		}

		public void Mediate (IMediated mediated)
		{
			var mediatedType = mediated.GetType();
			var hasMediators = Create(mediated, mediatedType);
			var mediatedInterfaces = mediatedType.GetInterfaces();

			for (int i = 0; i < mediatedInterfaces.Length; i++) {
				hasMediators |= Create(mediated, mediatedInterfaces[i]);
			}

			if (hasMediators) {
				mediated.OnMediation();
			}
			else {
				throw new NoMediatorMappingException("no mediators for " + mediatedType);
			}
		}

		bool Create<T> (T mediated, Type mediatedType) where T : IMediated
		{
			IList<Type> mediatorTypes;
			bool hasMediators = mediatingMap.TryGetValue(mediatedType, out mediatorTypes);

			if (hasMediators) {
				for (int i = 0; i < mediatorTypes.Count; i++) {
					Get(mediatorTypes[i]).Init(mediated);
				}
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
