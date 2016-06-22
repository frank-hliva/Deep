open System.IO
open System

type Request() =
    member r.Params = ["Controller", "Home"; "Action", "Index"] |> Map

type Response() = class
    end


type ViewData = Map<string, string>

[<AllowNullLiteral>]
type IView =
    abstract Render : vars : ViewData -> input : string -> string

type ViewPathFinder(viewRoot : string, ext : string) =

    member f.TryFind (parameters : Map<string, string>, path) =
        let fn = Path.ChangeExtension(path, ext)
        [
            Path.Combine(viewRoot, parameters.["Controller"], fn)
            Path.Combine(viewRoot, fn)
            Path.Combine(viewRoot, "Shared", fn)
            Path.Combine(fn)
        ] |> List.tryFind(File.Exists)

    member f.TryFind (parameters : Map<string, string>) =
        f.TryFind(parameters, parameters.["Action"])

type Reply(request : Request, response : Response, view : IView) =

    let getViewPath = function
    | Some path ->
        if path |> File.Exists then path
        else path
    | _ ->
        sprintf "/View/%s/%s.html" request.Params.["Controller"] request.Params.["Action"]
        |> fun path -> Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path)

    member private r.View(path : string option, viewData : ViewData option) =
        match view with
        | null -> failwith "View engine not found"
        | _ ->
        path
        |> getViewPath
        |> File.ReadAllText
        |> view.Render (defaultArg viewData Map.empty)
    member r.View(path : string, ?viewData : ViewData) = r.View(Some path, viewData)
    member r.View(?viewData : ViewData) = r.View(None, viewData)

let req = new Request()

let finder = new ViewPathFinder(@"c:\aaa\View", "html")
finder.TryFind(req.Params, "Index1")

let x = 1
let y = 2

let x = (=>)

if x >= y then "xxx" else "yyy"