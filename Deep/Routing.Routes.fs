namespace Deep.Routing

open Deep
open FSharp.Reflection

type Routes() =

    static let objToHandler : obj -> _ = function
    | :? IRouteHandler as h -> h
    | h when FSharpType.IsFunction <| h.GetType() -> FunctionRouteHandler(h) :> IRouteHandler
    | :? MvcDefaults as h -> MvcRouteHandler(h) :> IRouteHandler
    | _ -> failwith "Invalid route handler"

    static member AddRoute (route : route) (routes : routes) =
        routes @ [route]

    static member private AddBase(routes, httpMethod, pattern, handler, filter) =
        routes
        |> Routes.AddRoute
            {
                HttpMethod = httpMethod
                Pattern = pattern
                Handler = handler |> objToHandler
                Filter = filter
            }

    static member Add(routes, httpMethod, pattern, handler, ?filter) =
        Routes.AddBase(routes, httpMethod, pattern, handler, filter)

    static member Any(routes, pattern, handler, ?filter) =
        Routes.AddBase(routes, HttpMethods.Any, pattern, handler, filter)

    static member Get(routes, pattern, handler, ?filter) =
        Routes.AddBase(routes, HttpMethods.Get, pattern, handler, filter)

    static member Post(routes, pattern, handler, ?filter) =
        Routes.AddBase(routes, HttpMethods.Post, pattern, handler, filter)

    static member Put(routes, pattern, handler, ?filter) =
        Routes.AddBase(routes, HttpMethods.Put, pattern, handler, filter)

    static member Delete(routes, pattern, handler, ?filter) =
        Routes.AddBase(routes, HttpMethods.Delete, pattern, handler, filter)