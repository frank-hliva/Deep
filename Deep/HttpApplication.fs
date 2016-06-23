namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

[<AllowNullLiteral>]
type RequestKernelConfigurator(config : IKernel -> IKernel) =
    member c.Config = config

type HttpApplication(applicationKernel : IKernel, router : IRouter, requestConfigurator : RequestKernelConfigurator) =

    let registerRequestObjects (context : HttpListenerContext) (matchResult : RouteMatchResult) (requestContainer : IKernel) =
        [
            context |> box
            new Request(context.Request, matchResult.Parameters) |> box
            new Response(context.Response) |> box
        ]
        |> Seq.fold
            (fun (requestContainer : IKernel) (instance : obj) ->
                instance |> requestContainer.RegisterInstance) requestContainer
        |> fun requestContainer -> requestContainer.Register<Reply>(LifeTime.Singleton)

    member a.Container = applicationKernel

    member a.Listener(context : HttpListenerContext) =
        let request = context.Request
        match router.Match request.HttpMethod request.RawUrl with
        | Some result ->
            new Kernel(applicationKernel) :> IKernel
            |> registerRequestObjects context result
            |> fun kernel -> 
                match requestConfigurator with
                | null -> kernel
                | _ -> requestConfigurator.Config(kernel)
            |> fun kernel ->
                kernel |> result.Handler.InvokeAction
                match kernel.TryFindInstance(typedefof<Reply>) with
                | Some reply ->
                    let reply = reply :?> Reply
                    if not reply.IsDisposed then (reply :> IDisposable).Dispose()
                | _ -> ()
        | _ -> ()

    interface IApplication with
        override a.Run(uri : string) = Server.listen uri a.Listener

    new(applicationKernel, router) = HttpApplication(applicationKernel, router, null)