using System;

namespace MinMVC
{
	interface ITestClass
	{

	}

	[WaitForInit]
	class TestClass : ITestClass
	{

	}

	class InjectingTestClass
	{
		[Inject]
		public ITestInjection testInjection;
	}

	interface ITestInjection
	{

	}

	class TestInjection : ITestInjection
	{

	}

	public class TestMediator : Mediator<IMediatedView>
	{
		public IDispatcher dispatcher;

		protected override void Register ()
		{
			dispatcher.dispatch += _view.Remove;
		}

		protected override void Unregister ()
		{
			dispatcher.dispatch -= _view.Remove;
			dispatcher = null;
		}
	}

	public interface IDispatcher
	{
		Action dispatch { get; set; }
	}

	public class TestCommand : Command
	{
		public override void Execute ()
		{

		}
	}
}
