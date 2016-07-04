[<AutoOpen>]
module App.Config

open Castle.Windsor
open Castle.MicroKernel.Registration
open Deep
open Deep.Routing
open Deep.Mvc
open Deep.View.DotLiquid

let registerRoutes (routes : routes) =
    Routes.Any(
        routes,
        "/?Controller/?Action/?Id",
        { Controller = "Home"; Action = "Index"; Id = "" }
    )

type RouteBuilder(config : RouteBuilderConfig) =
    inherit Routing.RouteBuilder(registerRoutes, config)

let config (container : IWindsorContainer) =
    container
        .Register(Component.For<Config>().LifeStyle.Singleton)
        .Register(
            AllTypes
                .FromAssemblyNamed("Deep")
                .BasedOn<IConfigSection>()
                .WithServiceSelf()
                .LifestyleSingleton()
        ) |> ignore
    [
        (typedefof<IView>, typedefof<View>)
        (typedefof<IRouteBuilder>, typedefof<RouteBuilder>)
        (typedefof<Router>, typedefof<Router>)
        (typedefof<StaticContent>, typedefof<StaticContent>)
        (typedefof<ISessionStore>, typedefof<MemorySessionStore>)
    ] |> List.iter(fun (t1, t2) -> container.Register(Component.For(t1).ImplementedBy(t2).LifeStyle.Singleton) |> ignore)
    let listenerContainer =
        ListenerContainer()
            .Use(container.Resolve<StaticContent>())
            .Use(container.Resolve<Router>())
            .Use(ErrorHandler())
    container.Register(Component.For<ListenerContainer>().Instance(listenerContainer).LifeStyle.Singleton)