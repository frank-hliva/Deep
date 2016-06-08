namespace Deep.Routing

open Deep

type Router() =

    let delimiter = '/'
    let paramPrefix = ":"
    let optionalParamPrefix = "?"

    let parseByPrefix (prefix : string) (input : string) =
        if input.StartsWith(prefix)
        then Some(input.[1..])
        else None

    let (|Param|_|) (input : string) = input |> parseByPrefix paramPrefix
    let (|OptionalParam|_|) (input : string) = input |> parseByPrefix optionalParamPrefix

    let rec parseItems (i : int) (acc : RouteParams) (urlItems : string[]) = function
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

    let matchRoute (urlItems : string[]) (patternItems : string[]) =
        if urlItems.Length > patternItems.Length then None
        else patternItems |> List.ofSeq |> parseItems 0 Map.empty urlItems

    let matchChooser urlItems httpMethod' (route : route) =
        match route.Pattern.Split [| delimiter |] |> matchRoute urlItems with
        | Some parameters ->
            if route.HttpMethod = HttpMethods.Any || route.HttpMethod = httpMethod' then
                Some {
                    Handler = route.Handler
                    Parameters =
                        match route.Filter with
                        | Some filter -> filter(parameters)
                        | _ -> parameters
                }
            else None
        | _ -> None

    member r.Match (httpMethod : string) (url : string) (items : routes) =
        let urlItems = (url |> Url.removeQueryString).Split [| delimiter |]
        items |> List.tryPick (matchChooser urlItems httpMethod)