using UnityEngine;
using System;

namespace MinMVC
{
	public abstract class MediatedView : MonoBehaviour, IMediatedView
	{
		public event Action onRemove;

		protected virtual void Start()
		{
			Mediate();
		}

		protected virtual void OnDestroy()
		{
			onRemove();
		}

		public virtual void OnMediation()
		{

		}

		public virtual void Remove()
		{
			if(gameObject != null) {
				Destroy(gameObject);
			} else {
				OnDestroy();
			}
		}

		protected void Mediate()
		{
			if(transform.parent) {
				IRootView root = transform.parent.GetComponentInParent<IRootView>();

				if(root != null) {
					root.mediate(this);
				}
			}
		}
	}
}
