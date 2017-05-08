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
			context.RegisterClass<TestClass>();

			Assert.True(context.Has<TestClass>());
		}

		[Test]
		public void RegistersInterfaceByTemplate ()
		{
			context.RegisterClass<ITestClass, TestClass>();

			Assert.True(context.Has<ITestClass>());
			Assert.False(context.Has<TestClass>());

			var instance = context.Get<ITestClass>();
			Assert.NotNull(instance);
		}

		[Test]
		public void RegistersClassByType ()
		{
			var type = typeof(TestClass);
			context.RegisterType(type);

			Assert.True(context.Has(type));
		}

		[Test]
		public void RegistersInterfaceByType ()
		{
			var interfaceType = typeof(ITestClass);
			var classType = typeof(TestClass);
			context.RegisterType(interfaceType, classType);

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
			context.RegisterClass<TestClass>(true);
			var inst1 = context.Get<TestClass>();
			var inst2 = context.Get<TestClass>();
			var inst3 = context.Get<TestClass>();

			Assert.AreNotEqual(inst1, inst2);
			Assert.AreNotEqual(inst2, inst3);
		}

		[Test]
		public void ThrowsExceptionWhenTypeNotRegistered ()
		{
			Assert.Throws<NotRegistered>(() => context.Get<ITestClass>());
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
			Assert.Throws<NotRegistered>(() => context.Get<TextClassInjectingInterface>());
		}

		[Test]
		public void ThrowsCannotRegisterInterfaceAsValueException ()
		{
			Assert.Throws<CannotRegisterInterfaceAsValue>(() => context.RegisterType(typeof(ITestClass)));
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
		public void InjectInstancesInCircle ()
		{
			var instance1 = new CircularClass1();
			var instance2 = new CircularClass2();

			context.RegisterInstance(instance1);
			context.RegisterInstance(instance2);

			context.Get<CircularClass1>();
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
			context.RegisterClass<TestClass>();
			var instance = context.Get<TestClass>();

			Assert.False(instance.isCleanedUp);

			context.CleanUp();

			Assert.True(instance.isCleanedUp);
		}
	}
}
