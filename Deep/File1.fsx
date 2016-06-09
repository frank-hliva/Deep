open System
open System.Reflection

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type RouteFilterAttribute() =
    inherit Attribute()


let [<RouteFilter()>]  xxx () = ()

let findByAttribute<'t> (assemblies : Assembly seq) =
    assemblies
    |> Seq.collect(fun a -> a.GetTypes())
    |> Seq.collect(fun t -> t.GetMethods())
    |> Seq.filter(fun m -> m.GetCustomAttributes(typedefof<'t>, false).Length > 0)

let mi = [System.Reflection.Assembly.GetExecutingAssembly()] |> findByAttribute<RouteFilterAttribute> |> Seq.head

let getAttribute<'t> (m : MethodInfo) =
    m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

let tryGetAttribute<'t> (m : MethodInfo) =
    let attrs = m.GetCustomAttributes(typedefof<'t>, false)
    match attrs.Length with
    | 0 -> None
    | _ -> attrs |> Seq.head |> Some

mi |> tryGetAttribute<ReflectedDefinitionAttribute>