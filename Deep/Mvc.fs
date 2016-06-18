namespace Deep.Mvc

open Deep
open System.Net
open System.Reflection

[<AllowNullLiteral>]
type internal ControllerContext(httpListenerContext : HttpListenerContext, request : Request, response : Response) =
    member c.HttpListenerContext = httpListenerContext
    member c.Request = request
    member c.Response = request
    
type Controller() =
    let controllerContext : ControllerContext = null
    member internal c.ControllerContext = controllerContext
    member c.Request = controllerContext.Request
    member c.Response = controllerContext.Response

module internal ControllerModule =
    let setControllerContext (context : ControllerContext) (controller : Controller) =
        typedefof<Controller>
           .GetField("controllerContext", BindingFlags.Instance ||| BindingFlags.NonPublic)
           .SetValue(controller, context)

module Controllers =
    
    let findAll (assemblies : Assembly[]) =
        assemblies
        |> Seq.collect(fun a -> a.GetTypes())
        |> Seq.filter(fun t -> t.IsSubclassOf(typedefof<Controller>))

    let findByName (name : string) assemblies =
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

    let defaultVal (name : string) =
        let value = defaults.GetType().GetProperty(name).GetValue(defaults).ToString()
        value

    let getRouteParams (parameters : RouteParams) =
        ["Controller"; "Action"; "Id"]
        |> List.map
            (fun name ->
                match parameters |> Map.tryFind name with
                | Some value when value |> String.IsNullOrEmpty |> not -> name, value
                | _ -> name, name |> defaultVal)
        |> Map

    interface IRouteHandler with

        member h.InvokeAction(container : IKernel) =
            let request = container.Resolve<Request>()
            let parameters = request.Params |> getRouteParams |> Map.map(fun _ v -> v |> Url.toPascalCase)
            let mvcConfig = container.Resolve<MvcConfig>() :> IAssemblyConfig
            let controllerType =
                mvcConfig.GetAssemblies()
                |> Controllers.findByName (parameters.["Controller"] + "Controller")
            match controllerType with
            | Some controllerType ->
                let controller = container.Register(controllerType).Resolve(controllerType)
                controllerType.GetMethod(parameters.["Action"])
                |> Function.invokeOn controller (container.RegisterInstance<string>(parameters.["Id"]))
                |> ignore
            | _ -> ()