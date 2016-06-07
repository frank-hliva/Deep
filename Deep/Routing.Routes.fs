[<RequireQualifiedAccess>]
module Deep.Routing.Routes

open Deep
open FSharp.Reflection

let private delimiter = '/'

let private parseByPrefix (prefix : string) (input : string) =
    if input.StartsWith(prefix)
    then Some(input.[1..])
    else None

let private (|Param|_|) (input : string) = input |> parseByPrefix Prefix.Param
let private (|OptionalParam|_|) (input : string) = input |> parseByPrefix Prefix.OptionalParam

let rec private parseItems (i : int) (acc : ParamMap) (urlItems : string[]) = function
| [] -> Some acc
| x :: xs ->
    match (x : string) with
    | Param p ->
        if urlItems.Length - 1 < i then None
        else xs |> parseItems (i + 1) (acc.Add(p, urlItems.[i])) urlItems
    | OptionalParam p ->
        let value = if urlItems.Length - 1 < i then "" else urlItems.[i]
        xs |> parseItems (i + 1) (acc.Add(p, value)) urlItems
    | x ->
        if urlItems.Length - 1 < i || x <> urlItems.[i] then None
        else xs |> parseItems (i + 1) acc urlItems

let private matchRoute (urlItems : string[]) (patternItems : string[]) =
    if urlItems.Length > patternItems.Length then None
    else patternItems |> List.ofSeq |> parseItems 0 Map.empty urlItems

let addHandler (httpMethod : string) (pattern : string) (handler : IRouteHandler) (filter : ParamMapFilter option) (items : routes) =
    items @ [httpMethod, pattern, handler, filter]

let private addBase httpMethod pattern handler filter items =
    let handler =
        match handler with
        | h when FSharpType.IsFunction (h.GetType()) -> FunctionRouteHandler(handler) :> IRouteHandler
        | h when h.GetType() = typedefof<MvcDefaults> -> MvcRouteHandler(h) :> IRouteHandler
        | _ -> failwith "Invalid route handler"
    items |> addHandler httpMethod pattern handler filter

let add pattern handler items =
    items |> addBase HttpMethods.Any pattern handler None

let addWithFilter pattern handler items filter =
    items |> addBase HttpMethods.Any pattern handler (Some filter)

let private matchChooser urlItems httpMethod' ((httpMethod, pattern, handler, filter) : route) =
    match pattern.Split [| delimiter |] |> matchRoute urlItems with
    | Some parameters ->
        if httpMethod = HttpMethods.Any || httpMethod = httpMethod' then
            Some {
                Method = httpMethod
                Handler = handler
                Parameters =
                    match filter with
                    | Some filter -> filter(parameters)
                    | _ -> parameters
            }
        else None
    | _ -> None

let match' (httpMethod : string) (url : string) (items : routes) =
    let urlItems = (url |> Url.removeQueryString).Split [| delimiter |]
    items |> List.tryPick (matchChooser urlItems httpMethod)