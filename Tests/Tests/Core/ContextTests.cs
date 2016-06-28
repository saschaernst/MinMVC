using NUnit.Framework;

namespace MinMVC
{
	public class ContextTests
	{
		IContext _context;

		[SetUp]
		public void Setup ()
		{
			_context = new Context();
		}

		[Test]
		public void IsSetupCorrect ()
		{
			Assert.True(_context.Has<IContext>());
		}

		[Test]
		public void RegistersClassByTemplate ()
		{
			_context.Register<TestClass>();

			Assert.True(_context.Has<TestClass>());
		}

		[Test]
		public void RegistersInterfaceByTemplate ()
		{
			_context.Register<ITestClass, TestClass>();

			Assert.True(_context.Has<ITestClass>());
			Assert.False(_context.Has<TestClass>());
		}

		[Test]
		public void RegistersClassByType ()
		{
			var type = typeof(TestClass);
			_context.Register(type);

			Assert.True(_context.Has(type));
		}

		[Test]
		public void RegistersInterfaceByType ()
		{
			var interfaceType = typeof(ITestClass);
			var classType = typeof(TestClass);
			_context.Register(interfaceType, classType);

			Assert.True(_context.Has(interfaceType));
			Assert.False(_context.Has(classType));
		}

		[Test]
		public void RegistersInstanceAlwaysCached ()
		{
			ITestClass instance = new TestClass();
			_context.RegisterInstance(instance);

			Assert.True(_context.Has<ITestClass>());
			Assert.False(_context.Has<TestClass>());

			var retrieved = _context.Get<ITestClass>();

			Assert.AreEqual(instance, retrieved);
		}

		[Test]
		public void CachesInstances ()
		{
			_context.Register<TestClass>();
			var inst1 = _context.Get<TestClass>();
			var inst2 = _context.Get<TestClass>();

			Assert.AreEqual(inst1, inst2);
		}

		[Test]
		public void PreventsCaching ()
		{
			_context.Register<TestClass>(true);
			var inst1 = _context.Get<TestClass>();
			var inst2 = _context.Get<TestClass>();

			Assert.AreNotEqual(inst1, inst2);
		}

		[Test]
		public void ThrowsExceptionWhenTypeNotRegistered ()
		{
			Assert.Throws<NotRegisteredException>(() => _context.Get<TestClass>());
		}

		[Test]
		public void InjectsIntoFieldOfRegisteredInstance ()
		{
			_context.Register<FieldInjectionTestClass>();
			_context.Register<ITestInjection, TestInjection>();
			var instance = _context.Get<FieldInjectionTestClass>();

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void InjectsIntoPropertyOfRegisteredInstance ()
		{
			_context.Register<PropertyInjectionTestClass>();
			_context.Register<ITestInjection, TestInjection>();
			var instance = _context.Get<PropertyInjectionTestClass>();

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void ThrowsExceptionIfInjectionIsNotRegistered ()
		{
			_context.Register<FieldInjectionTestClass>();

			Assert.Throws<NotRegisteredException>(() => _context.Get<FieldInjectionTestClass>());
		}

		[Test]
		public void InjectsIntoNonRegisterdInstance ()
		{
			_context.Register<ITestInjection, TestInjection>();
			var instance = new FieldInjectionTestClass();
			_context.Inject(instance);

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void PutTaggedClassOnInitializingList ()
		{
			_context.Register<InitializingTestClass>();
			_context.Register<WaitForInitInjectingClass>();
			var instance = _context.Get<WaitForInitInjectingClass>();

			Assert.True(_context.IsInitializing(instance.testInjection));
			Assert.True(instance.postInjectHasBeenCalled);
			Assert.False(instance.postInitHasBeenCalled);

			_context.InitDone(instance.testInjection);

			Assert.True(instance.postInitHasBeenCalled);
		}
	}
}
