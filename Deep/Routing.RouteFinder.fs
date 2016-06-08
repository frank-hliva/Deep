[<AutoOpen>]
module Deep.Routing.RouteFinder

open Deep
open System.Reflection

let private getAttribute<'t> (m : MethodInfo) =
    m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

type Routes with

    static member AddMarkedActions (assemblies : Assembly seq) (routes : routes) =
        assemblies
        |> Function.findByAttribute<RouteAttribute>
        |> Seq.map
            (fun mi ->
                let attr = mi |> getAttribute<RouteAttribute>
                {
                    HttpMethod = attr.HttpMethod
                    Pattern = attr.RoutePattern
                    Handler = new FunctionRouteHandler(mi) :> IRouteHandler
                    Filter = None
                })
        |> fun r -> routes |> Seq.append r |> List.ofSeq