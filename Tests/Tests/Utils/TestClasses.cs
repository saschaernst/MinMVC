using System;

namespace MinMVC
{
	public interface ITestClass
	{

	}

	public class TestClass : ITestClass
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

	public class TestInjection : ITestInjection
	{

	}

	public class CleanupClass
	{
		public bool isCleanedUp;

		[Cleanup]
		public void Cleanup ()
		{
			isCleanedUp = true;
		}
	}

	public class PostInjectionClass
	{
		[Inject]
		public ITestInjection testInjection;

		public bool isPostInjected;

		[PostInjection]
		public void PostInjection ()
		{
			isPostInjected = true;
		}
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
