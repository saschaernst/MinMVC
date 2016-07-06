using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class CommandsTests
	{
		Commands commands;

		[SetUp]
		public void Setup ()
		{
			commands = new Commands();
			commands.context = Substitute.For<IContext>();
		}

		[Test]
		public void GetsCacheByType ()
		{
			var cache = commands.Get<TestCommand>();

			Assert.NotNull(cache);
			Assert.True(commands.Has<TestCommand>());
		}

		[Test]
		public void RemovesCacheByType ()
		{
			commands.Get<TestCommand>();
			commands.Remove<TestCommand>();

			Assert.False(commands.Has<TestCommand>());
		}
	}
}
