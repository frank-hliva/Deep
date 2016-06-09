namespace Deep.Routing

open Deep
open System
open System.Net

type IRouteHandler =
    abstract InvokeAction : obj list -> unit

type FunctionRouteHandler(func : obj) =
    interface IRouteHandler with
        member h.InvokeAction(parameters : obj list) =
            func |> Function.invoke parameters |> ignore

type MvcDefaults = { Controller : string; Action : string; Id : string }

type MvcRouteHandler(defaults : obj) =
    interface IRouteHandler with
        member h.InvokeAction(parameters : obj list) =
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