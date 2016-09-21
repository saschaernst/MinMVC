using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class NamedDispatcherTests
	{
		NamedDispatcher dispatcher;

		[SetUp]
		public void Setup ()
		{
			dispatcher = new NamedDispatcher();
			dispatcher.commands = Substitute.For<ICommands>();
		}

		[Test]
		public void RegistersNamesToCommands ()
		{
			dispatcher.Register<TestCommand>("bla");

			dispatcher.commands.Received().Get<TestCommand>();
		}

		[Test]
		public void ExecutesCommandsByRegisteredName ()
		{
			var cache = Substitute.For<ICommandCache>();
			dispatcher.commands.Get<TestCommand>().Returns(cache);
			const string name = "eventName";
			dispatcher.Register<TestCommand>(name);
			dispatcher.Execute(name);
			dispatcher.commands.Received(1).Get<TestCommand>();
			cache.Received(1).Execute();
		}
	}
}
