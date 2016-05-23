using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class NamedDispatcherTests
	{
		NamedDispatcher _dispatcher;
		ICommands _commands;

		[SetUp]
		public void Setup()
		{
			_dispatcher = new NamedDispatcher();
			_commands = _dispatcher.commands = Substitute.For<ICommands>();
		}

		[Test]
		public void RegistersNamesToCommands()
		{
			_dispatcher.Register<TestCommand>("bla");
			_commands.Received().Get<TestCommand>();
		}

		[Test]
		public void ExecutesCommandsByRegisteredName()
		{
			var cache = Substitute.For<ICommandCache>();
			_commands.Get<TestCommand>().Returns(cache);
			const string name = "eventName";
			_dispatcher.Register<TestCommand>(name);
			_dispatcher.Execute(name);

			_commands.Received(1).Get<TestCommand>();
			cache.Received(1).Execute();
		}
	}
}
