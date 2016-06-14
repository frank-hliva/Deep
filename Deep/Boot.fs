namespace Deep

open System
open Deep
open Deep.Routing

type [<AbstractClass>] Booter(kernel : IKernel) =
    let mutable container = kernel.RegisterInstance<IKernel>(kernel)
    abstract DefaultConfigurator : IKernel -> IKernel
    member b.Kernel = container
    member b.Config() =
        container <- b.DefaultConfigurator(container)
    member b.Config(configurator : IKernel -> IKernel) =
        b.Config()
        container <- configurator(container)
    abstract Boot : string -> unit

type ApplicationBooter<'t when 't : not struct and 't :> IApplication>(kernel) =
    inherit Booter(kernel)
    override b.DefaultConfigurator(kernel) =
        kernel
            .Register<IRouter, Router>(LifeTime.Singleton)
            .Register<'t>(LifeTime.Singleton)
    override b.Boot(uri : string) = b.Kernel.Resolve<'t>().Run(uri)