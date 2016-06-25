[<AutoOpen>]
module CelebAgency.Config

open Deep
open Deep.Routing
open Castle.Windsor
open Castle.MicroKernel.Registration
open Deep.Mvc

let registerRoutes (routes : routes) =
    Routes.Any(
        routes,
        "/?Controller/?Action/?Id",
        { Controller = "Home"; Action = "Index"; Id = "" }
    )

type RouteBuilder(config : RouteBuilderConfig) =
    inherit Deep.Routing.RouteBuilder(registerRoutes, config)

let config (container : IWindsorContainer) =
    container
        .Register(Component.For<Config>().LifeStyle.Singleton)
        .Register(Component.For<ControllerConfig>().LifeStyle.Singleton)
        .Register(Component.For<ViewConfig>().LifeStyle.Singleton)
        .Register(Component.For<IView>().ImplementedBy<Deep.View.DotLiquid.View>().LifeStyle.Singleton)
        .Register(Component.For<RouteBuilderConfig>().LifeStyle.Singleton)
        .Register(Component.For<IRouteBuilder>().ImplementedBy<RouteBuilder>().LifeStyle.Singleton)
        .Register(Component.For<Router>().LifeStyle.Singleton)
        .Register(Component.For<StaticContentConfig>().LifeStyle.Singleton)
        .Register(Component.For<StaticContent>().LifeStyle.Singleton)
        |> ignore
    let listenerContainer =
        ListenerContainer()
            .Use(container.Resolve<StaticContent>())
            .Use(container.Resolve<Router>())
    container.Register(Component.For<ListenerContainer>().Instance(listenerContainer).LifeStyle.Singleton)