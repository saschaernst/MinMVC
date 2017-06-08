using MinTools;
using UnityEngine;

namespace MinMVC
{
	public abstract class MediatedBehaviour : MonoBehaviour, IMediatedBehaviour
	{
		public MinSignal OnStart { get; private set; }
		public MinSignal OnEnabled { get; private set; }
		public MinSignal OnDisabled { get; private set; }
		public MinSignal OnRemove { get; private set; }

		protected virtual void Awake ()
		{
			OnStart = new MinSignal();
			OnEnabled = new MinSignal();
			OnDisabled = new MinSignal();
			OnRemove = new MinSignal();

			MediateBehaviour();
		}

		protected virtual void Start ()
		{
			OnStart.Call();
		}

		protected virtual void OnDestroy ()
		{
			OnRemove.Call();
			Cleanup();
		}

		protected virtual void OnEnable ()
		{
			OnEnabled.Call();
		}

		protected virtual void OnDisable ()
		{
			OnDisabled.Call();
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
			OnStart.Reset();
			OnEnabled.Reset();
			OnDisabled.Reset();
			OnRemove.Reset();
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
