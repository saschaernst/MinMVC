using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class MediatorsTests
	{
		Mediators _mediators;

		[SetUp]
		public void Setup ()
		{
			_mediators = new Mediators();
			_mediators.context = Substitute.For<IContext>();
		}

		[Test]
		public void MediatesView ()
		{
			var mediator = Substitute.For<TestMediator>();
			var view = Substitute.For<IMediatedView>();
			_mediators.context.Get<IMediator>(typeof(TestMediator)).Returns(mediator);
			_mediators.Map<IMediatedView, TestMediator>();
			_mediators.Mediate(view);

			mediator.Received(1).Init(view);
			view.Received(1).OnMediation();
		}

		[Test]
		public void ThrowsExceptionIfNoMediatorIsMapped ()
		{
			var view = Substitute.For<IMediatedView>();
			var noInstance = new TestDelegate(() => _mediators.Mediate(view));

			Assert.Throws<NoMediatorMappingException>(noInstance);
		}
	}
}
