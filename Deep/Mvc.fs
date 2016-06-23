namespace Deep.Mvc

open Deep
open System.Net
open System.Reflection

type ControllerConfig(config : Config) =
    inherit AssemblyConfig()
    override c.GetAssemblyNames() =
        config.SelectAs<string[]>("Controllers.Assemblies")

module Controllers =
    let internal suffix = "Controller"

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
            |> Controllers.tryFindByName (sprintf "%s%s" parameters.[MvcKeys.Controller] Controllers.suffix)
            |> function
            | Some controllerType ->
                let controller = container.Register(controllerType).Resolve(controllerType)
                match controllerType.GetMethod(parameters.[MvcKeys.Action]) with
                | null -> () |> RouteHandlerResult.toAsync
                | action ->
                    let container = container |> registerId parameters.[MvcKeys.Id]
                    action |> Function.invokeOn controller container |> RouteHandlerResult.toAsync
            | _ -> () |> RouteHandlerResult.toAsync