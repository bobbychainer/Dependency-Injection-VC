﻿using NUnit.Framework;

namespace VContainer.Tests
{
    [TestFixture]
    public class ContainerTest
    {
        [Test]
        public void ResolveTransient()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Transient);

            var container = builder.Build();
            var obj1 = container.Resolve<NoDependencyServiceA>();
            var obj2 = container.Resolve<NoDependencyServiceA>();

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.Not.EqualTo(obj2));
        }

        [Test]
        public void ResolveSingleton()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton);

            var container = builder.Build();
            var obj1 = container.Resolve<NoDependencyServiceA>();
            var obj2 = container.Resolve<NoDependencyServiceA>();

            Assert.That(obj1, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj2, Is.TypeOf<NoDependencyServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));
        }

        [Test]
        public void ResolveScoped()
        {
            var builder = new ContainerBuilder();
            builder.Register<DisposableServiceA>(Lifetime.Scoped);

            var container = builder.Build();
            var obj1 = container.Resolve<DisposableServiceA>();
            var obj2 = container.Resolve<DisposableServiceA>();

            Assert.That(obj1, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj2, Is.TypeOf<DisposableServiceA>());
            Assert.That(obj1, Is.EqualTo(obj2));

            container.Dispose();

            Assert.That(obj1.Disposed, Is.True);
        }

        [Test]
        public void ResolveAsInterfaces()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<MultipleInterfaceServiceA>(Lifetime.Singleton)
                .As<I1>()
                .As<I3>();

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj1, Is.EqualTo(obj3));
            Assert.Throws<VContainerException>(() => container.Resolve<MultipleInterfaceServiceA>());
        }

        [Test]
        public void ResolveAsInterfacesAndSelf()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<MultipleInterfaceServiceA>(Lifetime.Singleton)
                .AsSelf()
                .As<I1>()
                .As<I3>();

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<MultipleInterfaceServiceA>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<MultipleInterfaceServiceA>());
            Assert.That(obj4, Is.EqualTo(obj1));
            Assert.That(obj4, Is.EqualTo(obj3));
        }

        [Test]
        public void ResolveDependencies()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            builder.Register<I4, ServiceA>(Lifetime.Singleton);
            builder.Register<I5, ServiceB>(Lifetime.Singleton);
            builder.Register<I6, ServiceC>(Lifetime.Singleton);

            var container = builder.Build();

            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<I4>();
            var obj5 = container.Resolve<I5>();
            var obj6 = container.Resolve<I6>();

            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<I4>());
            Assert.That(obj5, Is.InstanceOf<I5>());
            Assert.That(obj6, Is.InstanceOf<I6>());
        }

        [Test]
        public void ResolveAllInjectableFeatures()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, AllInjectionFeatureService>(Lifetime.Singleton);
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.Register<I3, NoDependencyServiceB>(Lifetime.Singleton);
            builder.Register<I4, ServiceA>(Lifetime.Singleton);
            builder.Register<I5, ServiceB>(Lifetime.Singleton);
            builder.Register<I6, ServiceC>(Lifetime.Singleton);
            builder.Register<I7, ServiceD>(Lifetime.Singleton);

            var container = builder.Build();

            var obj1 = container.Resolve<I1>();
            var obj2 = container.Resolve<I2>();
            var obj3 = container.Resolve<I3>();
            var obj4 = container.Resolve<I4>();
            var obj5 = container.Resolve<I5>();
            var obj6 = container.Resolve<I6>();
            var obj7 = container.Resolve<I7>();

            Assert.That(obj1, Is.InstanceOf<I1>());
            Assert.That(obj2, Is.InstanceOf<I2>());
            Assert.That(obj3, Is.InstanceOf<I3>());
            Assert.That(obj4, Is.InstanceOf<I4>());
            Assert.That(obj5, Is.InstanceOf<I5>());
            Assert.That(obj6, Is.InstanceOf<I6>());
            Assert.That(obj7, Is.InstanceOf<I7>());

            Assert.That(((AllInjectionFeatureService)obj1).ConstructorCalled, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).Method1Called, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).Method2Called, Is.True);
            Assert.That(((AllInjectionFeatureService)obj1).GetPrivateProperty(), Is.InstanceOf<I2>());
            Assert.That(((AllInjectionFeatureService)obj1).PublicPropertyInjectable, Is.InstanceOf<I3>());
            Assert.That(((AllInjectionFeatureService)obj1).GetPrivateFieldInjectable(), Is.InstanceOf<I4>());
            Assert.That(((AllInjectionFeatureService)obj1).PublicFieldInjectable, Is.InstanceOf<I5>());
            Assert.That(((AllInjectionFeatureService)obj1).FromConstructor1, Is.InstanceOf<I6>());
            Assert.That(((AllInjectionFeatureService)obj1).FromConstructor2, Is.InstanceOf<I7>());
        }

        // TODO: コンパイル時に検知可能 ?
        [Test]
        [Ignore("Not implemented")]
        public void InvalidInterface()
        {
            var builder = new ContainerBuilder();
            builder.Register<NoDependencyServiceA>(Lifetime.Singleton).As<I7>();
            Assert.Throws<VContainerException>(() => builder.Build());
        }

        [Test]
        public void RegisterInstance()
        {
            var builder = new ContainerBuilder();
            var instance = new NoDependencyServiceB();
            builder.RegisterInstance<I3>(instance);

            var container = builder.Build();
            var obj1 = container.Resolve<I3>();
            var obj2 = container.Resolve<I3>();
            Assert.That(obj1, Is.EqualTo(instance));
            Assert.That(obj2, Is.EqualTo(instance));
        }
    }
}