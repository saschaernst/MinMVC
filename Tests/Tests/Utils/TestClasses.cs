﻿using System;

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
