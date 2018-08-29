namespace Deep

open System

type IApplication =
    abstract Run : string seq -> unit

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
type IgnoreActionAttribute() = inherit Attribute()

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

[<AutoOpen>]
module Operators =
    let (=>) a b = a, box b

exception HttpException of int * string

type IAutoDisposable =
    inherit IDisposable
    abstract IsDisposed : bool with get

module TypeConversion =

    let changeType (conversion : Type) (value : obj) =
        match value with
        | null -> null
        | _ ->
            (if conversion.IsGenericType && conversion.GetGenericTypeDefinition().Equals(typedefof<Nullable<_>>)
            then Nullable.GetUnderlyingType(conversion)
            else conversion) |> fun conversion -> Convert.ChangeType(value, conversion)

    let change<'t> (value : obj) =
        let conversion = typedefof<'t>
        match value with
        | null -> Unchecked.defaultof<'t>
        | _ ->
            (if conversion.IsGenericType && conversion.GetGenericTypeDefinition().Equals(typedefof<Nullable<_>>)
            then Nullable.GetUnderlyingType(conversion)
            else conversion) |> fun conversion -> Convert.ChangeType(value, conversion) :?> 't


module TimeSpan =

    let tryParse (input : string) =
        let items = input.Split([|':'|])
        let input =
            match items.Length with
            | 2 -> sprintf "%s:00" input
            | _ -> input
        let success, timeSpan = input |> TimeSpan.TryParse
        if success then Some timeSpan
        else None