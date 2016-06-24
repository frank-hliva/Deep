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

    abstract RegisterRequestObjects : HttpListenerContext -> IKernel -> IKernel
    default a.RegisterRequestObjects (context : HttpListenerContext) (requestContainer : IKernel) =
        [
            context |> box
            Request(context.Request) |> box
            new Response(context.Response) |> box
        ]
        |> Seq.fold
            (fun (requestContainer : IKernel) (instance : obj) ->
                instance |> requestContainer.RegisterInstance) requestContainer
        |> fun requestContainer -> requestContainer.Register<Reply>(LifeTime.Singleton)
        |> fun kernel -> 
            match requestConfigurator with
            | null -> kernel
            | _ -> requestConfigurator.Config(kernel)

    member internal a.AutoDisposeObjects(kernel : IKernel) =
        match kernel.TryFindInstance(typedefof<Reply>) with
        | Some reply ->
            let reply = reply :?> Reply
            if not reply.IsDisposed then (reply :> IDisposable).Dispose()
        | _ -> ()

    member a.Container = applicationKernel

    member a.Listener(context : HttpListenerContext) = async {
        let request = context.Request
        let kernel = new Kernel(applicationKernel) :> IKernel |> a.RegisterRequestObjects context
        match router.Match request.HttpMethod request.RawUrl with
        | Some result ->
            let kernel = kernel.RegisterInstance(new Request(context.Request, result.Parameters))
            do! result.Handler.InvokeAction kernel
            kernel |> a.AutoDisposeObjects
        | _ -> () }

    interface IApplication with
        override a.Run(uri : string) = Server.listen uri a.Listener

    new(applicationKernel, router) = HttpApplication(applicationKernel, router, null)