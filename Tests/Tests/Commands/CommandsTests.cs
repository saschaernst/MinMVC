using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class CommandsTests
	{
		Commands _commands;

		[SetUp]
		public void Setup()
		{
			_commands = new Commands();
			_commands.context = Substitute.For<IContext>();
		}

		[Test]
		public void GetsCacheByType()
		{
			var cache = _commands.Get<TestCommand>();

			Assert.NotNull(cache);
			Assert.True(_commands.Has<TestCommand>());
		}

		[Test]
		public void RemovesCacheByType()
		{
			_commands.Get<TestCommand>();
			_commands.Remove<TestCommand>();

			Assert.False(_commands.Has<TestCommand>());
		}
	}
}
