using MinTools;
using UnityEngine;

namespace MinMVC
{
	public abstract class MediatedBehaviour : MonoBehaviour, IMediated
	{
		public MinSignal OnStart { get; set; }
		public MinSignal OnEnabled { get; set; }
		public MinSignal OnDisabled { get; set; }
		public MinSignal OnRemove { get; set; }

		protected virtual void Awake ()
		{
			MediateBehaviour();
		}

		protected virtual void Start ()
		{
			if (OnStart != null) {
				OnStart.Call();
			}
		}

		protected virtual void OnDestroy ()
		{
			if (OnRemove != null) {
				OnRemove.Call();
				Cleanup();
			}
		}

		protected virtual void OnEnable ()
		{
			if (OnEnabled != null) {
				OnEnabled.Call();
			}
		}

		protected virtual void OnDisable ()
		{
			if (OnDisabled != null) {
				OnDisabled.Call();
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
			OnEnabled = null;
			OnDisabled = null;
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
