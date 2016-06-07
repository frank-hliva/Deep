module Deep.Routing.RouteFinder

open Deep
open System.Reflection

let private getAttribute<'t> (m : MethodInfo) =
    m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

let findMarkedFunctions (assemblies : Assembly seq) (routes : routes) =
    assemblies
    |> Function.findByAttribute<RouteAttribute>
    |> Seq.map
        (fun mi ->
            let attr = mi |> getAttribute<RouteAttribute>
            attr.HttpMethod, attr.RoutePattern, new FunctionRouteHandler(mi) :> IRouteHandler, None)
    |> fun r -> routes |> Seq.append r |> List.ofSeq