namespace Deep.Routing

open Deep
open System.Net

module Prefix =
    let Param = ":"
    let OptionalParam = "?"

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

type ParamMap = Map<string, string>
type ParamMapFilter = ParamMap -> ParamMap

type route = string * string * IRouteHandler * ParamMapFilter option
type routes = route list
type MatchResult = { Method: string; Handler: IRouteHandler; Parameters: ParamMap }