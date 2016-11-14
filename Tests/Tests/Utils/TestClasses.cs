using System;

namespace MinMVC
{
	public interface ITestClass
	{

	}

	public class TestClass : ITestClass
	{
		public bool isPostInjected;
		public bool isCleanedUp;

		[Inject]
		public TestInjection fieldInjection;

		[Inject]
		public TestInjection propertyInjection { get; set; }

		[PostInjection]
		public void PostInjection ()
		{
			isPostInjected = true;
		}

		[Cleanup]
		public void Cleanup ()
		{
			isCleanedUp = true;
		}
	}

	public class TextClassInjectingInterface
	{
		[Inject]
		public ITestClass injection;
	}

	public class TestInjection
	{

	}

	public class CircularClass1
	{
		[Inject]
		public CircularClass2 circular;
	}

	public class CircularClass2
	{
		[Inject]
		public CircularClass1 circular;
	}

	public class TestMediator : Mediator<IMediated>
	{
		public IDispatcher dispatcher;

		protected override void Register ()
		{
			dispatcher.dispatch += mediated.Remove;
		}

		protected override void Unregister ()
		{
			dispatcher.dispatch -= mediated.Remove;
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
