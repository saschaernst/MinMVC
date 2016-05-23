using NSubstitute;
using NUnit.Framework;

namespace MinMVC
{
	public class CommandCacheTests
	{
		[Test]
		public void ExecutesCommand ()
		{
			var context = Substitute.For<IContext>();
			var command = Substitute.For<TestCommand>();
			context.Get<TestCommand>().Returns(command);
			var cache = new CommandCache<TestCommand>(context, false);
			cache.Execute();

			context.Received(1).Register<TestCommand>(true);
			command.Received(1).Execute();
		}
	}
}
