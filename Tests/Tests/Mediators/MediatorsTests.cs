using NUnit.Framework;
using NSubstitute;

namespace MinMVC
{
	public class MediatorsTests
	{
		Mediators mediators;

		[SetUp]
		public void Setup ()
		{
			mediators = new Mediators();
			mediators.context = Substitute.For<IContext>();
		}

		[Test]
		public void MediatesView ()
		{
			var mediator = Substitute.For<TestMediator>();
			var view = Substitute.For<IMediated>();
			mediators.context.Get<IMediator>(typeof(TestMediator)).Returns(mediator);
			mediators.Map<IMediated, TestMediator>();
			mediators.Mediate(view);

			mediator.Received(1).Init(view);
			view.Received(1).OnMediation();
		}

		[Test]
		public void ThrowsExceptionIfNoMediatorIsMapped ()
		{
			var view = Substitute.For<IMediated>();
			var noInstance = new TestDelegate(() => mediators.Mediate(view));

			Assert.Throws<NoMediatorMappingException>(noInstance);
		}
	}
}
