using NSubstitute;
using NUnit.Framework;

namespace MinMVC
{
	public class CommandCacheTests
	{
		[Test]
		public void ExecutesCommand ()
		{
			var commands = Substitute.For<Commands>();
			var command = Substitute.For<TestCommand>();
			commands.GetCommand<TestCommand>().Returns(command);
			var cache = new CommandCache<TestCommand>(commands);
			cache.Execute();

			commands.Received(1).GetCommand<TestCommand>();
			command.Received(1).Execute();

			cache.Execute();

			commands.Received(1).GetCommand<TestCommand>();
			command.Received(2).Execute();
		}
	}
}
