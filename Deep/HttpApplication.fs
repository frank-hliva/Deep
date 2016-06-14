namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

[<AllowNullLiteral>]
type RequestKernelConfigurator(config : IKernel -> IKernel) =
    member c.Config = config

type [<AbstractClass>] HttpApplication(applicationKernel : IKernel, router : IRouter, requestConfigurator : RequestKernelConfigurator) =

    let routes = []

    let registerDefaultObjects (context : HttpListenerContext) (matchResult : RouteMatchResult) (requestContainer : IKernel) =
        [
            context |> box
            new Request(context.Request, matchResult.Parameters) |> box
            new Response(context.Response) |> box
        ] |> Seq.fold
            (fun (requestContainer : IKernel) (instance : obj) ->
                requestContainer.RegisterInstance(instance)) requestContainer

    let proccessResult (context : HttpListenerContext) (matchResult : RouteMatchResult option) =
        match matchResult with
        | Some result ->
            new Kernel(applicationKernel) :> IKernel
            |> registerDefaultObjects context result
            |> fun kernel -> 
                match requestConfigurator with
                | null -> kernel
                | _ -> requestConfigurator.Config(kernel)
            |> result.Handler.InvokeAction
        | _ -> ()

    abstract RegisterRoutes : routes -> routes

    member a.Container = applicationKernel

    member a.Listener(context : HttpListenerContext) =
        let req = context.Request
        a.RegisterRoutes(routes)
        |> router.Match req.HttpMethod req.RawUrl
        |> proccessResult context

    interface IApplication with
        override a.Run(uri : string) = Server.listen uri a.Listener

    new(applicationKernel, router) = HttpApplication(applicationKernel, router, null)