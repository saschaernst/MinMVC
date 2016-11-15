using NUnit.Framework;

namespace MinMVC
{
	public class ContextTests
	{
		IContext context;

		[SetUp]
		public void Setup ()
		{
			context = new Context(InjectionCheck.Exception, true);
		}

		[Test]
		public void IsSetupCorrect ()
		{
			Assert.True(context.Has<IContext>());
		}

		[Test]
		public void RegistersClassByTemplate ()
		{
			context.Register<TestClass>();

			Assert.True(context.Has<TestClass>());
		}

		[Test]
		public void RegistersInterfaceByTemplate ()
		{
			context.Register<ITestClass, TestClass>();

			Assert.True(context.Has<ITestClass>());
			Assert.False(context.Has<TestClass>());

			var instance = context.Get<ITestClass>();
			Assert.NotNull(instance);
		}

		[Test]
		public void RegistersClassByType ()
		{
			var type = typeof(TestClass);
			context.Register(type);

			Assert.True(context.Has(type));
		}

		[Test]
		public void RegistersInterfaceByType ()
		{
			var interfaceType = typeof(ITestClass);
			var classType = typeof(TestClass);
			context.Register(interfaceType, classType);

			Assert.True(context.Has(interfaceType));
			Assert.False(context.Has(classType));
		}

		[Test]
		public void RegistersInstanceAlwaysCached ()
		{
			var instance = new TestClass();
			context.RegisterInstance(instance);

			Assert.True(context.Has<TestClass>());

			var retrieved = context.Get<TestClass>();

			Assert.AreEqual(instance, retrieved);
			Assert.NotNull(instance.fieldInjection);
		}

		[Test]
		public void CachesInstances ()
		{
			var inst1 = context.Get<TestClass>();
			var inst2 = context.Get<TestClass>();

			Assert.AreEqual(inst1, inst2);
		}

		[Test]
		public void PreventsCaching ()
		{
			context.Register<TestClass>(true);
			var inst1 = context.Get<TestClass>();
			var inst2 = context.Get<TestClass>();
			var inst3 = context.Get<TestClass>();

			Assert.AreNotEqual(inst1, inst2);
			Assert.AreNotEqual(inst2, inst3);
		}

		[Test]
		public void ThrowsExceptionWhenTypeNotRegistered ()
		{
			Assert.Throws<NotRegisteredException>(() => context.Get<ITestClass>());
		}

		[Test]
		public void InjectsIntoFieldOfRegisteredInstance ()
		{
			var instance = context.Get<TestClass>();

			Assert.NotNull(instance.fieldInjection);
		}

		[Test]
		public void InjectsIntoPropertyOfRegisteredInstance ()
		{
			var instance = context.Get<TestClass>();

			Assert.NotNull(instance.propertyInjection);
		}

		[Test]
		public void ThrowsExceptionIfInjectionIsNotRegistered ()
		{
			Assert.Throws<NotRegisteredException>(() => context.Get<TextClassInjectingInterface>());
		}

		[Test]
		public void ThrowsCannotRegisterInterfaceAsValueException ()
		{
			Assert.Throws<CannotRegisterInterfaceAsValueException>(() => context.Register(typeof(ITestClass)));
		}

		[Test]
		public void InjectsIntoNonRegisterdInstance ()
		{
			var instance = new TestClass();
			context.Inject(instance);

			Assert.NotNull(instance.fieldInjection);
		}

		[Test]
		public void InjectsInCircle ()
		{
			var instance1 = context.Get<CircularClass1>();

			Assert.NotNull(instance1);
		}

		[Test]
		public void CallsPostInjection ()
		{
			var instance = context.Get<TestClass>();

			Assert.True(instance.isPostInjected);
		}

		[Test]
		public void CleansupContext ()
		{
			context.Register<TestClass>();
			var instance = context.Get<TestClass>();

			Assert.False(instance.isCleanedUp);

			context.CleanUp();

			Assert.True(instance.isCleanedUp);
		}
	}
}
