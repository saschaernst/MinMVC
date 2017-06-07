using MinTools;

namespace MinMVC
{
	public interface IMediated
	{
		//MinSignal OnStart { get; set; }
		//MinSignal OnEnabled { get; set; }
		//MinSignal OnDisabled { get; set; }
		//MinSignal OnRemove { get; set; }

		void OnMediation ();

		void Remove ();
	}
}
