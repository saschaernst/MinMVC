using System;

namespace MinMVC
{
	public interface ITestClass
	{

	}

	public class TestClass : ITestClass
	{

	}

	[WaitForInit]
	public class InitializingTestClass : ITestClass
	{

	}

	public class FieldInjectionTestClass
	{
		[Inject]
		public ITestInjection testInjection;
	}

	public class PropertyInjectionTestClass
	{
		[Inject]
		public ITestInjection testInjection { get; set; }
	}

	public interface ITestInjection
	{

	}

	public class WaitForInitInjectingClass
	{
		[Inject]
		public InitializingTestClass testInjection;

		public int postInjects;
		public int postInits;

		[PostInjection]
		public void OnPostInject ()
		{
			postInjects++;
		}

		[PostInit]
		public void OnPostInit ()
		{
			postInits++;
		}
	}

	public class TestInjection : ITestInjection
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
