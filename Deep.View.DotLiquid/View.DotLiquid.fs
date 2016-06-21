namespace Deep.View.DotLiquid

open Deep
open System.IO
open System.Text
open DotLiquid

type View(viewConfig : ViewConfig, viewPathFinder : ViewPathFinder) =

    let toRenderParameters = function
    | Some (viewData : ViewData) -> 
        viewData
        |> Map.fold
            (fun (acc : RenderParameters) k v ->
                acc.LocalVariables.Add(k, v)
                acc) (new RenderParameters())
    | _ -> new RenderParameters()

    interface IView with
        override v.Render(routeParams, path, viewData) =
            path
            |> function
            | Some path -> viewPathFinder.TryFind(routeParams, path)
            | _ -> viewPathFinder.TryFind(routeParams)
            |> function
            | Some path -> 
                File.ReadAllText(path, Encoding.UTF8)
                |> fun text -> Template.Parse(text).Render(viewData |> toRenderParameters)
            | _ -> failwith "Invalid path"

    new(viewConfig : ViewConfig) =
        View(viewConfig, new ViewPathFinder(viewConfig))