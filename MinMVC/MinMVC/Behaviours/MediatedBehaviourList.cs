using UnityEngine;
using System.Collections.Generic;

namespace MinMVC
{
	public class MediatedBehaviourList : MonoBehaviour
	{
		[SerializeField] bool mediateRecursively;
		[SerializeField] GameObject[] mediatedObjects;

		public IList<IMediated> MediatedList
		{
			get {
				var list = new List<IMediated>();

				foreach (var mediatedObject in mediatedObjects) {
					var mediatedBehaviours = mediateRecursively
						? mediatedObject.GetComponentsInChildren<IMediated>()
						: mediatedObject.GetComponents<IMediated>();
					list.AddRange(mediatedBehaviours);
				}

				return list;
			}
		}
	}
}
