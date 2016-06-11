module CelebAgency.Container

open Deep
open Castle.Windsor
open Castle.MicroKernel.Registration

type TestClass() =
    member t.Member = "Fero Hliva"

let config (container : IWindsorContainer) =
    container.Register(Component.For<TestClass>().ImplementedBy<TestClass>().LifeStyle.Singleton)
    ()