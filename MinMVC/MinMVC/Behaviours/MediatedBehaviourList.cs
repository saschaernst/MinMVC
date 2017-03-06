using System.Collections.Generic;
using UnityEngine;

namespace MinMVC
{
	public class MediatedBehaviourList : MonoBehaviour
	{
		[SerializeField]
		bool mediateRecursively;
		[SerializeField]
		bool includeInactive;
		[SerializeField]
		GameObject[] mediatedObjects;

		public IList<IMediated> MediatedList {
			get {
				var list = new List<IMediated>();

				var speedo = new Timing();
				var ticket = speedo.Start();

				foreach (var mediatedObject in mediatedObjects) {
					var mediatedBehaviours = mediateRecursively
						? mediatedObject.GetComponentsInChildren<IMediated>(includeInactive)
						: mediatedObject.GetComponents<IMediated>();
					list.AddRange(mediatedBehaviours);
				}

				var result = speedo.Stop(ticket);

				return list;
			}
		}
	}
}
