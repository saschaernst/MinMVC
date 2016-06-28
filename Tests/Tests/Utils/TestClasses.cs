﻿using System;

namespace MinMVC
{
	interface ITestClass
	{

	}

	class TestClass : ITestClass
	{

	}

	[WaitForInit]
	class InitializingTestClass : ITestClass
	{

	}

	class FieldInjectionTestClass
	{
		[Inject]
		public ITestInjection testInjection;
	}

	class PropertyInjectionTestClass
	{
		[Inject]
		public ITestInjection testInjection { get; set; }
	}

	interface ITestInjection
	{

	}

	class WaitForInitInjectingClass
	{
		[InjectAndWait]
		public InitializingTestClass testInjection;

		public bool postInjectHasBeenCalled;

		public bool postInitHasBeenCalled;

		[PostInjection]
		public void OnPostInject ()
		{
			postInjectHasBeenCalled = true;
		}

		[PostInit]
		public void OnPostInit ()
		{
			postInitHasBeenCalled = true;
		}
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
