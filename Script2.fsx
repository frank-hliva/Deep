open System
open System.Reflection

[<AllowNullLiteral>]
[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type RouteAttribute(httpMethod : string, routePattern : string, priority : int) =
    inherit Attribute()
    member a.HttpMethod = httpMethod
    member a.RoutePattern = routePattern
    member a.Priority = priority

[<RequireQualifiedAccess>]
module HttpMethods =
    let Any = "ANY"
    let Get = "GET"
    let Post = "POST"
    let Put = "PUT"
    let Delete = "DELETE"

[<RequireQualifiedAccess>]
module ContentTypes =
    let html = "text/html"
    let plainText = "text/plain"
    let xml = "application/xml"
    let json = "application/json"

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type AnyAttribute(routePattern : string, priority : int) =
    inherit RouteAttribute(HttpMethods.Any, routePattern, priority)
    new(routePattern) = AnyAttribute(routePattern, 0)
    new() = AnyAttribute("")

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type GetAttribute(routePattern : string, priority : int) =
    inherit RouteAttribute(HttpMethods.Get, routePattern, priority)
    new(routePattern) = GetAttribute(routePattern, 0)
    new() = GetAttribute("")

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type PostAttribute(routePattern : string, priority : int) =
    inherit RouteAttribute(HttpMethods.Post, routePattern, priority)
    new(routePattern) = PostAttribute(routePattern, 0)
    new() = PostAttribute("")

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type PutAttribute(routePattern : string, priority : int) =
    inherit RouteAttribute(HttpMethods.Put, routePattern, priority)
    new(routePattern) = PutAttribute(routePattern, 0)
    new() = PutAttribute("")

[<System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)>]
type DeleteAttribute(routePattern : string, priority : int) =
    inherit RouteAttribute(HttpMethods.Delete, routePattern, priority)
    new(routePattern) = DeleteAttribute(routePattern, 0)
    new() = DeleteAttribute("")

type Controller() =
    member x.Test1() =
        "Test1"

    [<Put>]
    member x.Test1(post : string) =
        "Test1Post"

let rec findMethod (methodName : string) (httpMethod : string) (controllerType : Type) =
    let methods =
        controllerType.GetMethods(BindingFlags.Public ||| BindingFlags.Instance ||| BindingFlags.DeclaredOnly)
        |> Seq.filter(fun mi -> mi.Name = methodName)
    match methods
        |> Seq.tryFind
            (fun mi ->
                let routeAttr = mi.GetCustomAttribute<RouteAttribute>()
                match httpMethod with
                | "" -> routeAttr = null || routeAttr.HttpMethod = HttpMethods.Any
                | _ -> routeAttr <> null && routeAttr.HttpMethod = httpMethod) with
    | Some mi -> Some mi
    | None when httpMethod <> "" -> controllerType |> findMethod methodName ""
    | None -> None


let controller = new Controller()
controller.GetType() |> findMethod "Test1" "DELETE"