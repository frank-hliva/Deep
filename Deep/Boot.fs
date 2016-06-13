namespace Deep

open System
open Deep
open Deep.Routing

type [<AbstractClass>] Booter(kernel : IKernel) =
    let mutable container = kernel.RegisterInstance<IKernel>(kernel)
    abstract DefaultConfigurator : IKernel -> IKernel
    member b.Kernel = container
    member b.Config(?configurator : IKernel -> IKernel) =
        container <- b.DefaultConfigurator(container)
        match configurator with
        | Some configurator -> container <- configurator(container)
        | _ -> ()
    abstract Boot : string -> unit

type ApplicationBooter<'t when 't : not struct and 't :> HttpApplication>(kernel) =
    inherit Booter(kernel)
    override b.DefaultConfigurator(kernel) =
        kernel
            .Register<IRouter, Router>()
            .Register<'t>()
    override b.Boot(uri : string) = b.Kernel.Resolve<'t>().Run(uri)