using UnityEngine;
using System;

namespace MinMVC
{
	public abstract class MediatedView : MonoBehaviour, IMediatedView
	{
		public event Action OnRemove;

		protected virtual void Start ()
		{
			MediateView();
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

		protected void MediateView ()
		{
			if (transform.parent) {
				IRootView root = transform.parent.GetComponentInParent<IRootView>();

				if (root != null) {
					root.Mediate(this);
				}
			}
		}
	}
}
