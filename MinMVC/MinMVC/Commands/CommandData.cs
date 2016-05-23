namespace MinMVC
{
	public class CommandData<T>
	{
		public T value { get; private set; }

		public CommandData (T v)
		{
			value = v;
		}
	}
}
