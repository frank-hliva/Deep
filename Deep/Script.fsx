#load "Vega.fs"
#load "Url.fs"
#load "Function.fs"
#load "Routing.fs"
#load "Routing.Routes.fs"

open Vega
open Vega.Routing
open System
open System.IO
open System.Linq
open System.Reflection

[<Any("/?controller")>]
let controllerHandler (outputStream : StreamWriter) (p : Routing.ParamMap) =
    use stream = outputStream
    stream.WriteLine("Hello " + p.["controller"])
    |> ignore


let assemblies = AppDomain.CurrentDomain.GetAssemblies()

let getAttribute<'t> (m : MethodInfo) =
    m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

module Auto =
    let getAttribute<'t> (m : MethodInfo) =
        m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

    let addMarkedFunctions (assemblies : Assembly seq) (routes : routes) =
        assemblies
        |> Function.findByAttribute<RouteAttribute>
        |> Seq.map
            (fun mi ->
                let attr = mi |> getAttribute<RouteAttribute>
                attr.RoutePattern, new FunctionRouteHandler(mi) :> IRouteHandler, None)
        |> fun r -> routes |> Seq.append r

[]
|> Routes.add "/xxx/yyy" (fun x -> ())
|> Auto.addMarkedFunctions [assemblies.[15]]
let yyy = xxx.[0]
let qqq = yyy |> getAttribute<RouteAttribute>



