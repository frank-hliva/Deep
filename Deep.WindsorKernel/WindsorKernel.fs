namespace Deep

open Deep
open Castle.Windsor
open Castle.MicroKernel.Registration

type WindsorKernel(container : IWindsorContainer) =

    interface IKernel with

        member k.Register(t1, t2) =
            container.Register(Component.For(t1).ImplementedBy(t2))
            |> ignore

        member k.Register(t) =
            container.Register(Component.For(t).ImplementedBy(t))
            |> ignore

        member k.Resolve<'t>() =
            container.Resolve<'t>()
            
        member k.Resolve<'t>(key : string) =
            container.Resolve<'t>(key)

    member k.Container = container

    new() = WindsorKernel(new WindsorContainer())