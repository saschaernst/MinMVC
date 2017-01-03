using UnityEngine;
using System;

namespace MinMVC
{
	public abstract class MediatedBehaviour : MonoBehaviour, IMediated
	{
		public event Action OnStart;
		public event Action<bool> OnActivated;
		public event Action OnRemove;

		protected virtual void Awake ()
		{
			MediateBehaviour();
		}

		protected virtual void Start ()
		{
			if (OnStart != null) {
				OnStart();
			}
		}

		protected virtual void OnDestroy ()
		{
			if (OnRemove != null) {
				OnRemove();
				Cleanup();
			}
		}

		protected virtual void OnEnable ()
		{
			if (OnActivated != null) {
				OnActivated(true);
			}
		}

		protected virtual void OnDisable ()
		{
			if (OnActivated != null) {
				OnActivated(false);
			}
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

		protected virtual void Cleanup ()
		{
			OnStart = null;
			OnActivated = null;
			OnRemove = null;
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
