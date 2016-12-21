using UnityEngine;
using System;

namespace MinMVC
{
	public abstract class MediatedBehaviour : MonoBehaviour, IMediated
	{
		public event Action OnStart;
		public event Action OnRemove;

		protected virtual void Awake()
		{
			MediateBehaviour();
		}

		protected virtual void Start ()
		{
			OnStart();
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
			var root = transform.GetComponentInParent<IMediating>();

			if (root != null && !root.Equals(this)) {
				root.Mediate(this);
			}
		}
	}
}
