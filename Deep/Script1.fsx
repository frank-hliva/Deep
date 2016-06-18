open System
open System.Net
open System.Reflection
   
type Controller() = class
    end

 type BaseController() =
    inherit Controller()

 type Controller1() =
    inherit BaseController()

let assemblies =
    AppDomain.CurrentDomain.GetAssemblies()
    |> Array.find(fun x -> x.FullName.StartsWith("FSI-ASSEMBLY"))
    |> fun x -> [| x |]

module Controllers =
    
    let findAll (assemblies : Assembly[]) =
        assemblies
        |> Seq.collect(fun a -> a.GetTypes())
        |> Seq.filter(fun t -> t.IsSubclassOf(typedefof<Controller>))

    let findByName (name : string) assemblies =
        assemblies
        |> findAll
        |> Seq.tryFind (fun c -> c.Name = name)


assemblies
|> Controllers.findByName "Controller1"

type MvcDefaults = { Controller : string; Action : string; Id : string }

let defaults = { Controller = "Home"; Action = "Index"; Id = "" }

let getRouteParams (parameters : Map<string, string>) =
    ["Controller"; "Action"; "Id"]
    |> List.map
        (fun name ->
            match parameters |> Map.tryFind name with
            | Some value -> name, value
            | _ -> name, defaults.GetType().GetProperty(name).GetValue(defaults).ToString())
    |> Map

getRouteParams Map.empty