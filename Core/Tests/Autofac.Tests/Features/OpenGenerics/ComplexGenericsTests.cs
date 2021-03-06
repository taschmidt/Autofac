﻿using Autofac.Core.Registration;
using NUnit.Framework;
using System;

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class ComplexGenericsTests
    {
        // ReSharper disable UnusedTypeParameter, InconsistentNaming
        public interface IDouble<T2, T3> { }
        public class CReversed<T2, T1> : IDouble<T1, T2> { }

        public interface INested<T> { }
        public class Wrapper<T> { }
        public class CNested<T> : INested<Wrapper<T>> { }

        public class CNestedDerived<T> : CNested<T> { }

        public class CNestedDerivedReversed<TX, TY> : IDouble<TY, INested<Wrapper<TX>>> { }
        public class SameTypes<TA, TB> : IDouble<TA, INested<IDouble<TB, TA>>> { }
        // ReSharper restore UnusedTypeParameter, InconsistentNaming

        [Test]
        public void NestedGenericInterfacesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNested<>)).As(typeof(INested<>));
            var container = cb.Build();

            var nest = container.Resolve<INested<Wrapper<string>>>();
            Assert.IsInstanceOf<CNested<string>>(nest);
        }

        [Test]
        public void NestedGenericClassesCanBeResolved()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNestedDerived<>)).As(typeof(CNested<>));
            var container = cb.Build();

            var nest = container.Resolve<CNested<Wrapper<string>>>();
            Assert.IsInstanceOf<CNestedDerived<Wrapper<string>>>(nest);
        }

        [Test]
        public void CanResolveImplementationsWhereTypeParametersAreReordered()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var repl = container.Resolve<IDouble<int, string>>();
            Assert.IsInstanceOf<CReversed<string, int>>(repl);
        }

        [Test]
        public void CanResolveConcreteTypesThatReorderImplementedInterfaceParameters()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>));
            var container = cb.Build();

            var self = container.Resolve<CReversed<int, string>>();
            Assert.IsInstanceOf<CReversed<int, string>>(self);
        }

        [Test]
        public void TestNestingAndReversingSimplification()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CNestedDerivedReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
            Assert.IsInstanceOf<CNestedDerivedReversed<string, int>>(compl);
        }

        [Test]
        public void TestReversingWithoutNesting()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(CReversed<,>)).As(typeof(IDouble<,>));
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<Wrapper<string>>>>();
            Assert.IsInstanceOf<CReversed<INested<Wrapper<string>>, int>>(compl);
        }

        [Test]
        public void TheSameaceholderTypeCanAppearMultipleTimesInTheService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
            var container = cb.Build();

            var compl = container.Resolve<IDouble<int, INested<IDouble<string, int>>>>();
            Assert.IsInstanceOf<SameTypes<int, string>>(compl);
        }

        [Test]
        public void WhenTheSameTypeAppearsMultipleTimesInTheImplementationMappingItMustAlsoInTheService()
        {
            var cb = new ContainerBuilder();
            cb.RegisterGeneric(typeof(SameTypes<,>)).As(typeof(SameTypes<,>).GetInterfaces());
            var container = cb.Build();

            Assert.Throws<ComponentNotRegisteredException>(() =>
                container.Resolve<IDouble<decimal, INested<IDouble<string, int>>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstraint<T> { }

        public class Constrained<T1, T2> where T2 : IConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Test]
        public void CanResolveComponentWithNestedConstraintViaInterface()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Constrained<,>));

            var container = builder.Build();

            Assert.That(container.IsRegistered<Constrained<int, IConstraint<int>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstraintWithAddedArgument<T2, T1> : IConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Test]
        public void CanResolveComponentWhenConstrainedArgumentIsGenericTypeWithMoreArgumentsThanGenericConstraint()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(Constrained<,>));

            var container = builder.Build();

            Assert.That(container.IsRegistered<Constrained<int, IConstraintWithAddedArgument<string, int>>>());
        }

        // ReSharper disable UnusedTypeParameter
        public interface IConstrainedConstraint<T> where T : IEquatable<int> { }

        public interface IConstrainedConstraintWithAddedArgument<T1, T2> : IConstrainedConstraint<T2> where T2 : IEquatable<int> { }

        public interface IConstrainedConstraintWithOnlyAddedArgument<T1> : IConstrainedConstraintWithAddedArgument<T1, int> { }

        public class MultiConstrained<T1, T2> where T1 : IEquatable<int> where T2 : IConstrainedConstraint<T1> { }
        // ReSharper restore UnusedTypeParameter

        [Test]
        public void CanResolveComponentWhenConstraintsAreNested()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(MultiConstrained<,>));

            var container = builder.Build();

            Assert.That(container.IsRegistered<MultiConstrained<int, IConstrainedConstraintWithOnlyAddedArgument<string>>>());
        }
    }
}

namespace FluentValidation
{
    interface IValidator<T> { }
    class AbstractValidator<T> : IValidator<T> { }
}

namespace CompanyA
{
    interface IValidator<T> { } // Please note that this name is the same as FluentValidation's IValidator<T>
    class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidator<T> { }
}

namespace CompanyB
{
    interface IValidatorSomeOtherName<T> { } // This is NOT the same name.
    class CompositeValidator<T> : FluentValidation.AbstractValidator<T>, IValidatorSomeOtherName<T> { }
}

namespace Autofac.Tests.Features.OpenGenerics
{
    [TestFixture]
    public class ComplexGenericsWithNamepsaceTests
    {
        [Test]
        public void CanResolveByGenericInterface()
        {
            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(CompanyA.CompositeValidator<>)).As(typeof(CompanyA.IValidator<>));

            var container = builder.Build();

            var validator = container.Resolve<CompanyA.IValidator<int>>();
            Assert.That(validator, Is.InstanceOf<CompanyA.CompositeValidator<int>>());
        }
    }
}