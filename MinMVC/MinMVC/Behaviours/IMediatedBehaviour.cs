using MinTools;

namespace MinMVC
{
	public interface IMediatedBehaviour : IMediated
	{
		MinSignal OnStart { get; }
		MinSignal OnEnabled { get; }
		MinSignal OnDisabled { get; }
		MinSignal OnRemove { get; }
	}
}
