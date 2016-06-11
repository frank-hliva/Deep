namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO
open Castle.Windsor
open Castle.MicroKernel.Registration

type [<AbstractClass>] HttpApplication(applicationContainer : IWindsorContainer, router : IRouter) =

    let routes = []

    abstract RegisterRoutes : routes -> routes

    member a.Container = applicationContainer

    member a.ProccessResult (context : HttpListenerContext) (matchResult : RouteMatchResult option) =
        match matchResult with
        | Some result ->
            use requestContainer = new WindsorContainer() :> IWindsorContainer
            applicationContainer.AddChildContainer(requestContainer)
            requestContainer
                .Register(Component.For<HttpListenerContext>().Instance(context).LifestylePerThread())
                .Register(Component.For<Request>().Instance(new Request(context.Request, result.Parameters)).LifestylePerThread())
                .Register(Component.For<Response>().Instance(new Response(context.Response)).LifestylePerThread())
                |> ignore
            result.Handler.InvokeAction(requestContainer)
            applicationContainer.RemoveChildContainer(requestContainer)
        | _ -> ()

    member a.Listener(context : HttpListenerContext) =
        let req = context.Request
        a.RegisterRoutes(routes)
        |> router.Match req.HttpMethod req.RawUrl
        |> a.ProccessResult context

    member a.Run(uri : string) =
        Server.listen uri a.Listener