namespace Deep

open System
open System.IO
open Deep.Routing

type ViewData = Map<string, string>

[<AllowNullLiteral>]
type IView =
    abstract Render : routeParams : RouteParams * path : string option * viewData : ViewData option -> string

type ViewOptions = { Extension : string; RootDirectory : string }

type ViewConfig(config : Config) =
    member c.GetViewOptions() =
        let options = config.SelectAs<ViewOptions>("View")
        let baseDir = AppDomain.CurrentDomain.BaseDirectory
        { options with RootDirectory = options.RootDirectory.Replace("~/", baseDir) }

type ViewPathFinder(viewOptions : ViewOptions) =

    member f.TryFind (parameters : Map<string, string>, path) =
        let fn = Path.ChangeExtension(path, viewOptions.Extension)
        [
            Path.Combine(viewOptions.RootDirectory, parameters.["Controller"], fn)
            Path.Combine(viewOptions.RootDirectory, fn)
            Path.Combine(viewOptions.RootDirectory, "Shared", fn)
            Path.Combine(fn)
        ] |> List.tryFind(File.Exists)

    member f.TryFind (parameters : Map<string, string>) =
        f.TryFind(parameters, parameters.["Action"])

    new(viewConfig : ViewConfig) = ViewPathFinder(viewConfig.GetViewOptions())