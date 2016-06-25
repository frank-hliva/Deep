namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

[<AllowNullLiteral>]
type RequestKernelConfigurator(config : IKernel -> IKernel) =
    member c.Config = config

type HttpApplication(applicationKernel : IKernel, listenerContainer : ListenerContainer, requestConfigurator : RequestKernelConfigurator) =

    abstract RegisterRequestObjects : HttpListenerContext -> IKernel -> IKernel
    default a.RegisterRequestObjects (context : HttpListenerContext) (requestContainer : IKernel) =
        [
            context |> box
            new Request(context.Request) |> box
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

    member a.Container = applicationKernel

    member a.Listener(context : HttpListenerContext) =
        let kernel = new Kernel(applicationKernel) :> IKernel |> a.RegisterRequestObjects context
        listenerContainer.Apply(kernel)

    interface IApplication with
        override a.Run(uri : string) = Server.listen uri a.Listener

    new(applicationKernel, router) = HttpApplication(applicationKernel, router, null)