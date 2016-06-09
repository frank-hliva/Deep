[<AutoOpen>]
module Deep.Routing.RouteFinder

open Deep
open System.Reflection

type Routes with

    static member AddMarkedActions (assemblies : Assembly seq) (routes : routes) =
        assemblies
        |> Function.findByAttribute<RouteAttribute>
        |> Seq.map
            (fun mi ->
                let attr = mi |> Function.getAttribute<RouteAttribute>
                {
                    HttpMethod = attr.HttpMethod
                    Pattern = attr.RoutePattern
                    Handler = new FunctionRouteHandler(mi) :> IRouteHandler
                    Filter = None
                    Priority = attr.Priority
                })
        |> fun r -> routes |> Seq.append r |> List.ofSeq