open System



let addDefaults (defaults : Map<string, string>) (parameters : Map<string, string>) =
    defaults
    |> Map.fold
        (fun (acc : Map<string, string>) k v ->
            match acc.TryFind k with
            | Some value when String.IsNullOrEmpty value -> acc.Add(k, v)
            | None -> acc.Add(k, v)
            | _ -> acc) parameters

let defaults = Map ["Controller", "Home"; "Action", "Index"; "Id", ""]
let input = Map ["Controller", "xxxx"; "Action", ""; "Id", ""; "xxx", "yyy"]

let p =  input |> addDefaults defaults
