namespace MinMVC
{
	public class BehaviourMediator<T> : Mediator<T> where T: class, IMediatedBehaviour
	{
		protected override void Register ()
		{
			base.Register();

			RegisterSignal(mediated.OnStart, OnStart);
			RegisterSignal(mediated.OnEnabled, OnEnabled);
			RegisterSignal(mediated.OnDisabled, OnDisabled);
			RegisterSignal(mediated.OnRemove, OnRemove);
		}

		protected virtual void OnStart ()
		{

		}

		protected virtual void OnEnabled ()
		{

		}

		protected virtual void OnDisabled ()
		{

		}

		protected virtual void OnRemove ()
		{
			Cleanup();
		}
	}
}
