using NUnit.Framework;
using System;
using NSubstitute;

namespace MinMVC
{
	public class MediatorTests
	{
		IMediated view;
		TestMediator mediator;

		[SetUp]
		public void Setup ()
		{
			view = Substitute.For<IMediated>();
			mediator = new TestMediator();
			mediator.dispatcher = Substitute.For<IDispatcher>();
			mediator.Init(view);
		}

		[Test]
		public void RegistersListeners ()
		{
			mediator.dispatcher.dispatch();

			view.Received(1).Remove();
		}

		[Test]
		public void UnregistersOnViewRemoval ()
		{
			view.When(v => v.Remove()).Do(v => view.OnRemove += Raise.Event<Action>());
			mediator.dispatcher.dispatch();

			Assert.Null(mediator.dispatcher);
		}
	}
}
