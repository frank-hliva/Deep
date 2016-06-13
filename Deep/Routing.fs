namespace Deep.Routing

open Deep
open System
open System.Net

type IRouteHandler =
    abstract InvokeAction : container : IKernel -> unit

type FunctionRouteHandler(func : obj) =
    interface IRouteHandler with
        member h.InvokeAction(container : IKernel) =
            func |> Function.invoke container |> ignore

type MvcDefaults = { Controller : string; Action : string; Id : string }

type MvcRouteHandler(defaults : MvcDefaults) =
    interface IRouteHandler with
        member h.InvokeAction(container : IKernel) =
            ()

type RouteParams = Map<string, string>
type RouteParamFilter = RouteParams -> RouteParams

type route =
    {
        HttpMethod : string
        Pattern : string
        Handler : IRouteHandler
        Priority : int
        Filter : RouteParamFilter option
    }

type routes = route list

type RouteMatchResult =
    {
        Handler: IRouteHandler
        Parameters: RouteParams
    }

type IRouter =
    abstract Match : string -> string -> routes -> RouteMatchResult option