namespace Deep

open System
open Deep
open Deep.Routing
open Castle.Windsor
open Castle.MicroKernel.Registration

type [<AbstractClass>] Booter(container : IWindsorContainer) =
    do container.Register(Component.For<IWindsorContainer>().Instance(container)) |> ignore
    abstract DefaultConfigurator : IWindsorContainer -> unit
    member b.Kernel = container
    member b.Config(configurator : IWindsorContainer -> unit) =
        b.DefaultConfigurator(container)
        configurator(container)
    abstract Boot : string -> unit

type ApplicationBooter<'t when 't :> HttpApplication>(container) =
    inherit Booter(container)
    override b.DefaultConfigurator(container) =
        ()
    override b.Boot(uri : string) =
        let parameters : obj[] = [| container; new Router() |]
        let app = Activator.CreateInstance(typedefof<'t>, parameters) :?> HttpApplication
        app.Run(uri)
        ()