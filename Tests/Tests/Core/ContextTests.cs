using NUnit.Framework;
using System;

namespace MinMVC
{
	public class ContextTests
	{
		IContext context;

		[SetUp]
		public void Setup ()
		{
			context = new Context("", null, true);
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
		public void CachesRegisteredClass ()
		{
			context.Register<TestClass>();
			var instanceA = context.Get<TestClass>();
			var instanceB = context.Get<TestClass>();

			Assert.AreEqual(instanceA, instanceB);
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
			context.Register<ITestInjection, TestInjection>();
			var instance = new FieldInjectionTestClass();
			context.RegisterInstance(instance);

			Assert.True(context.Has<FieldInjectionTestClass>());

			var retrieved = context.Get<FieldInjectionTestClass>();

			Assert.AreEqual(instance, retrieved);
			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void CachesInstances ()
		{
			context.Register<TestClass>();
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
			Assert.Throws<NotRegisteredException>(() => context.Get<TestClass>());
		}

		[Test]
		public void InjectsIntoFieldOfRegisteredInstance ()
		{
			context.Register<FieldInjectionTestClass>();
			context.Register<ITestInjection, TestInjection>();
			var instance = context.Get<FieldInjectionTestClass>();

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void InjectsIntoPropertyOfRegisteredInstance ()
		{
			context.Register<PropertyInjectionTestClass>();
			context.Register<ITestInjection, TestInjection>();
			var instance = context.Get<PropertyInjectionTestClass>();

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void ThrowsExceptionIfInjectionIsNotRegistered ()
		{
			context.Register<FieldInjectionTestClass>();

			Assert.Throws<NotRegisteredException>(() => context.Get<FieldInjectionTestClass>());
		}

		[Test]
		public void InjectsIntoNonRegisterdInstance ()
		{
			context.Register<ITestInjection, TestInjection>();
			var instance = new FieldInjectionTestClass();
			context.Inject(instance);

			Assert.NotNull(instance.testInjection);
		}

		[Test]
		public void InjectsInCircle ()
		{
			context.Register<CircularClass1>();
			context.Register<CircularClass2>();
			var instance1 = context.Get<CircularClass1>();

			Assert.NotNull(instance1);
		}

		[Test]
		public void CallsPostInjection ()
		{
			context.Register<ITestInjection, TestInjection>();
			context.Register<PostInjectionClass>();
			var instance = context.Get<PostInjectionClass>();

			Assert.True(instance.isPostInjected);
		}

		[Test]
		public void CleansupContext ()
		{
			context.Register<CleanupClass>();
			var instance = context.Get<CleanupClass>();

			Assert.False(instance.isCleanedUp);

			context.CleanUp();

			Assert.True(instance.isCleanedUp);
		}
	}
}
