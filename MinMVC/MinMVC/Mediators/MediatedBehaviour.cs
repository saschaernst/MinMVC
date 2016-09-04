using UnityEngine;
using System;

namespace MinMVC
{
	public abstract class MediatedBehaviour : MonoBehaviour, IMediated
	{
		public event Action OnRemove;

		protected virtual void Start ()
		{
			MediateBehaviour();
		}

		protected virtual void OnDestroy ()
		{
			OnRemove();
		}

		public virtual void OnMediation ()
		{

		}

		public virtual void Remove ()
		{
			if (gameObject != null) {
				Destroy(gameObject);
			}
			else {
				OnDestroy();
			}
		}

		protected void MediateBehaviour ()
		{
			if (transform.parent) {
				IMediatedRoot root = transform.parent.GetComponentInParent<IMediatedRoot>();

				if (root != null) {
					root.Mediate(this);
				}
			}
		}
	}
}
