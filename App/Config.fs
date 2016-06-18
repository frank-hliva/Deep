[<AutoOpen>]
module App.Config

open Deep
open Castle.Windsor
open Castle.MicroKernel.Registration
open Deep.Routing

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
        .Register(Component.For<Config>().Instance(new Config(@"c:\Projekty\Deep\App\Config.json")).LifeStyle.Singleton)
        .Register(Component.For<RouteBuilderConfig>().LifeStyle.Singleton)
        .Register(Component.For<IRouteBuilder>().ImplementedBy<RouteBuilder>().LifeStyle.Singleton)
        .Register(Component.For<MvcConfig>().LifeStyle.Singleton)
        .Register(Component.For<IRouter>().ImplementedBy<Router>().LifeStyle.Singleton)