namespace Deep.Mvc

open Deep
open System.Net
open System.Reflection

type ControllerConfig(config : Config) =
    inherit AssemblyConfig()
    override c.GetAssemblyNames() =
        config.SelectAs<string[]>("Controllers.Assemblies")

module Controller =
    let suffix = "Controller"

    let findAll (assemblies : Assembly[]) =
        assemblies
        |> Seq.collect(fun a -> a.GetTypes())
        |> Seq.filter(fun t -> t.Name.EndsWith suffix)

    let tryFindByName (name : string) assemblies =
        assemblies
        |> findAll
        |> Seq.tryFind (fun c -> c.Name = name)

namespace Deep.Routing

open System
open System.Reflection
open Deep
open Deep.Mvc

type MvcDefaults = { Controller : string; Action : string; Id : string }

[<RequireQualifiedAccess>]
module internal MvcKeys =
    let Controller = "Controller"
    let Action = "Action"
    let Id = "Id"

type ControllerMethodType =
| Required = 0
| Optional = 1

type MvcRouteHandler() =

    let tryRegisterInt32Id (id : string) (container : IKernel) =
        match id.TryConvertToInt32() with
        | Some i -> container.RegisterInstance<Int32>(i)
        | _ -> container

    let tryRegisterDecimalId (id : string) (container : IKernel) =
        match id.TryConvertToDecimal() with
        | Some i -> container.RegisterInstance<Decimal>(i)
        | _ -> container

    let registerId (id : string) (container : IKernel) =
        container.RegisterInstance<string>(id)
        |> tryRegisterInt32Id id
        |> tryRegisterDecimalId id

    interface IRouteHandler with

        override h.InvokeAction(container : IKernel) =
            let request = container.Resolve<Request>()
            let parameters = request.Params |> Map.map(fun _ v -> v |> Url.toPascalCase)
            (container.Resolve<ControllerConfig>() :> IAssemblyConfig).GetAssemblies()
            |> Controller.tryFindByName (sprintf "%s%s" parameters.[MvcKeys.Controller] Controller.suffix)
            |> function
            | Some controllerType -> async {
                let controller = container.Register(controllerType).Resolve(controllerType)
                for (methodName, methodType) in 
                    [
                        ("Loaded", ControllerMethodType.Optional)
                        ("BeforeAction", ControllerMethodType.Optional)
                        (parameters.[MvcKeys.Action], ControllerMethodType.Required)
                        ("AfterAction", ControllerMethodType.Optional)
                    ] do
                    match controllerType.GetMethod(methodName) with
                    | null -> if methodType = ControllerMethodType.Required then () else ()
                    | action ->
                        let container = container |> registerId parameters.[MvcKeys.Id]
                        do! (action |> Function.invokeOn controller (container.RegisterInstance<IKernel> container) |> RouteHandlerResult.toAsync) }
            | _ -> () |> RouteHandlerResult.toAsync

namespace Deep.Mvc

open Deep
open Deep.Routing
open System.Net

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Controller =
    
    let executeAction (kernel : IKernel) (path : string) =
        let items = path.Split [| '/' |]
        let controller, action, id =
            match items.Length with
            | 3 -> items.[0], items.[1], items.[2]
            | 2 -> items.[0], items.[1], ""
            | _ -> failwith "invalid path"
        let mvcRouteHandler = new MvcRouteHandler() :> IRouteHandler
        let context = kernel.Resolve<HttpListenerContext>()
        let routeParams =
            Map [
                MvcKeys.Controller, controller
                MvcKeys.Action, action
                MvcKeys.Id, id
            ]
        kernel
            .RegisterInstance<Request>(new Request(context.Request, routeParams))
            .Register<Reply>(LifeTime.Singleton)
        |> mvcRouteHandler.InvokeAction