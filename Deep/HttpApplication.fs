namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

type [<AbstractClass>] HttpApplication(applicationKernel : IKernel, router : IRouter) =

    let routes = []

    let registerDefaultObjects (context : HttpListenerContext) (matchResult : RouteMatchResult) (requestContainer : IKernel) =
        [
            context |> box
            new Request(context.Request, matchResult.Parameters) |> box
            new Response(context.Response) |> box
        ] |> Seq.fold
            (fun (requestContainer : IKernel) (instance : obj) ->
                requestContainer.RegisterInstance(instance)) requestContainer

    abstract RegisterRoutes : routes -> routes

    member a.Container = applicationKernel

    member a.ProccessResult (context : HttpListenerContext) (matchResult : RouteMatchResult option) =
        match matchResult with
        | Some result ->
            new Kernel(applicationKernel) :> IKernel
            |> registerDefaultObjects context result
            |> result.Handler.InvokeAction
        | _ -> ()

    member a.Listener(context : HttpListenerContext) =
        let req = context.Request
        a.RegisterRoutes(routes)
        |> router.Match req.HttpMethod req.RawUrl
        |> a.ProccessResult context

    member a.Run(uri : string) =
        Server.listen uri a.Listener