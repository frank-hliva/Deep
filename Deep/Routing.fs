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

type IRouteBuilder =
    abstract Routes : routes

type SimpleRouteBuilder(builder : routes -> routes) =
    let routes = [] |> builder
    interface IRouteBuilder with
        override b.Routes = routes

type IRouter =
    abstract Match : string -> string -> RouteMatchResult option