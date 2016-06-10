namespace Deep

open System
open Deep
open Deep.Routing

type [<AbstractClass>] Booter(kernel : IKernel) =
    do kernel.RegisterInstance(kernel)
    abstract DefaultConfigurator : IKernel -> unit
    member b.Kernel = kernel
    member b.Config(configurator : IKernel -> unit) =
        b.DefaultConfigurator(kernel)
        configurator(kernel)
    abstract Boot : string -> unit

type ApplicationBooter<'t when 't :> HttpApplication>(kernel) =
    inherit Booter(kernel)
    override b.DefaultConfigurator(kernel) =
        ()
    override b.Boot(uri : string) =
        let parameters : obj[] = [| kernel; new Router() |]
        let app = Activator.CreateInstance(typedefof<'t>, parameters) :?> HttpApplication
        app.Run(uri)
        ()