using NUnit.Framework;
using System;
using NSubstitute;

namespace MinMVC
{
	public class MediatorTests
	{
		IMediatedView _view;
		TestMediator _mediator;

		[SetUp]
		public void Setup()
		{
			_view = Substitute.For<IMediatedView>();
			_mediator = new TestMediator();
			_mediator.dispatcher = Substitute.For<IDispatcher>();
			_mediator.Init(_view);
		}

		[Test]
		public void RegistersListeners()
		{
			_mediator.dispatcher.dispatch();

			_view.Received(1).Remove();
		}

		[Test]
		public void UnregistersOnViewRemoval()
		{
			_view.When(v => v.Remove()).Do(v => _view.onRemove += Raise.Event<Action>());
			_mediator.dispatcher.dispatch();

			Assert.Null(_mediator.dispatcher);
		}
	}
}
