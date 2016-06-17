[<AutoOpen>]
module App.Config

open Deep
open Castle.Windsor
open Castle.MicroKernel.Registration
open Deep.Routing

let config (container : IWindsorContainer) =
    container
        .Register(Component.For<IConfigSource>().Instance(new ConfigFileSource(@"c:\Projekty\Deep\App\Config.json")).LifeStyle.Singleton)
        .Register(Component.For<Config>().LifeStyle.Singleton)
        .Register(Component.For<AttributeRouteBuilderConfig>().LifeStyle.Singleton)
        .Register(Component.For<IRouteBuilder>().ImplementedBy<AttributeRouteBuilder>().LifeStyle.Singleton)
        .Register(Component.For<ControllerConfig>().LifeStyle.Singleton)
        .Register(Component.For<IRouter>().ImplementedBy<Router>().LifeStyle.Singleton)