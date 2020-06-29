﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

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

        [Test]
        public void ResolveCollection()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);
            builder.Register<I1, MultipleInterfaceServiceB>(Lifetime.Transient);

            var container = builder.Build();
            var enumerable = container.Resolve<IEnumerable<I1>>();
            var e0 = enumerable.ElementAt(0);
            var e1 = enumerable.ElementAt(1);
            Assert.That(e0, Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(e1, Is.TypeOf<MultipleInterfaceServiceB>());

            var list = container.Resolve<IReadOnlyList<I1>>();
            Assert.That(list[0], Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(list[1], Is.TypeOf<MultipleInterfaceServiceB>());

            // Singleton
            Assert.That(list[0], Is.EqualTo(e0));

            // Empty
            var empty = container.Resolve<IEnumerable<I7>>();
            Assert.That(empty, Is.Empty);
        }

        [Test]
        public void ResolveOnceAsCollection()
        {
            var builder = new ContainerBuilder();
            builder.Register<I1, MultipleInterfaceServiceA>(Lifetime.Singleton);

            var container = builder.Build();
            var enumerable = container.Resolve<IEnumerable<I1>>();
            var e0 = enumerable.ElementAt(0);
            Assert.That(e0, Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(enumerable.Count(), Is.EqualTo(1));

            var list = container.Resolve<IReadOnlyList<I1>>();
            Assert.That(list[0], Is.TypeOf<MultipleInterfaceServiceA>());
            Assert.That(list.Count, Is.EqualTo(1));
        }

        [Test]
        public void RegisterInstance()
        {
            var builder = new ContainerBuilder();
            var instance1 = new NoDependencyServiceB();
            var instance2 = new MultipleInterfaceServiceA();
            builder.RegisterInstance<I3>(instance1);
            builder.RegisterInstance<I1, I2>(instance2);

            var container = builder.Build();

            var resolve1a = container.Resolve<I3>();
            var resolve1b = container.Resolve<I3>();
            Assert.That(resolve1a, Is.EqualTo(instance1));
            Assert.That(resolve1b, Is.EqualTo(instance1));
            Assert.Throws<VContainerException>(() => container.Resolve<NoDependencyServiceB>());

            var resolve2a = container.Resolve<I1>();
            var resolve2b = container.Resolve<I2>();
            Assert.That(resolve2a, Is.EqualTo(instance2));
            Assert.That(resolve2b, Is.EqualTo(instance2));
            Assert.Throws<VContainerException>(() => container.Resolve<MultipleInterfaceServiceA>());
        }

        [Test]
        public void RegisterMultipleDisposables()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDisposable, DisposableServiceA>(Lifetime.Scoped);
            builder.Register<IDisposable, DisposableServiceB>(Lifetime.Scoped);

            var container = builder.Build();
            var disposables = container.Resolve<IReadOnlyList<IDisposable>>();
            container.Dispose();

            Assert.That(disposables[0], Is.TypeOf<DisposableServiceA>());
            Assert.That(disposables[1], Is.TypeOf<DisposableServiceB>());
            Assert.That(disposables[0], Is.InstanceOf<IDisposable>());
            Assert.That(disposables[1], Is.InstanceOf<IDisposable>());
            Assert.That(((DisposableServiceA)disposables[0]).Disposed, Is.True);
            Assert.That(((DisposableServiceB)disposables[1]).Disposed, Is.True);
        }

        [Test]
        public void RegisterWithParameter()
        {
            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Scoped)
                    .WithParameter<I2>(paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<ServiceA>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<ServiceA>(Lifetime.Scoped)
                    .WithParameter("service2", paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<ServiceA>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<HasMethodInjection>(Lifetime.Scoped)
                    .WithParameter<I2>(paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<HasMethodInjection>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }

            {
                var paramValue = new NoDependencyServiceA();
                var builder = new ContainerBuilder();
                builder.Register<HasMethodInjection>(Lifetime.Scoped)
                    .WithParameter("service2", paramValue);

                var container = builder.Build();
                var resolved = container.Resolve<HasMethodInjection>();
                Assert.That(resolved.Service2, Is.EqualTo(paramValue));
            }
        }

        [Test]
        public void RegisterSystem()
        {
            var builder = new ContainerBuilder();
            builder.Register<I2, NoDependencyServiceA>(Lifetime.Singleton);
            builder.RegisterContainer();

            var container = builder.Build();
            var resolve = container.Resolve<IObjectResolver>();
            Assert.That(resolve, Is.EqualTo(container));
        }

        [Test]
        public void RegisterConflictImplementations()
        {
            var builder = new ContainerBuilder();
            builder.Register<IDisposable, DisposableServiceA>(Lifetime.Scoped);
            builder.Register<IDisposable, DisposableServiceA>(Lifetime.Scoped);

            Assert.Throws<VContainerException>(() => builder.Build());
        }

        [Test]
        public void RegisterInvalidInterface()
        {
            var builder = new ContainerBuilder();
            Assert.Throws<VContainerException>(() => builder.Register<NoDependencyServiceA>(Lifetime.Scoped).As<I1>());
        }

        [Test]
        public void ErrorMessageIncludesDependency()
        {
            var builder = new ContainerBuilder();
            builder.Register<AllInjectionFeatureService>(Lifetime.Scoped);

            var container = builder.Build();
            Assert.Throws<VContainerException>(() => container.Resolve<AllInjectionFeatureService>(),
                "Failed to resolve VContainer.Tests.AllInjectionFeatureService : No such registration of type: VContainer.Tests.I6");
       }

        [Test]
        public void CircularDependency()
        {
            var builder = new ContainerBuilder();
            builder.Register<HasCircularDependency1>(Lifetime.Transient);
            builder.Register<HasCircularDependency2>(Lifetime.Transient);

            // Assert.Throws<AggregateException>(() => builder.Build());
            Assert.Throws<VContainerException>(() => builder.Build(),
                "Circular dependency detected! type: VContainer.Tests.HasCircularDependency1");
        }
    }
}
