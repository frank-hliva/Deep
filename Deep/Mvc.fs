namespace Deep.Mvc

open Deep
open System.Net
open System.Reflection

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

type MvcRouteHandler(defaults : MvcDefaults) =

    let controllerKey = "Controller"
    let actionKey = "Action"
    let IdKey = "Id"

    let defaultVal (name : string) =
        let value = defaults.GetType().GetProperty(name).GetValue(defaults).ToString()
        value

    let getRouteParams (parameters : RouteParams) =
        [controllerKey; actionKey; IdKey]
        |> List.map
            (fun name ->
                match parameters |> Map.tryFind name with
                | Some value when value |> String.IsNullOrEmpty |> not -> name, value
                | _ -> name, name |> defaultVal)
        |> Map

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

        member h.InvokeAction(container : IKernel) =
            let request = container.Resolve<Request>()
            let parameters = request.Params |> getRouteParams |> Map.map(fun _ v -> v |> Url.toPascalCase)
            (container.Resolve<MvcConfig>() :> IAssemblyConfig).GetAssemblies()
            |> Controllers.tryFindByName (sprintf "%s%s" parameters.[controllerKey] Controllers.suffix)
            |> function
            | Some controllerType ->
                let controller = container.Register(controllerType).Resolve(controllerType)
                match controllerType.GetMethod(parameters.[actionKey]) with
                | null -> ()
                | action ->
                    let container = container |> registerId parameters.[IdKey]
                    action |> Function.invokeOn controller container |> ignore
            | _ -> ()