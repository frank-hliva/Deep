namespace Deep.View.DotLiquid

open Deep
open System.IO
open System.Text
open DotLiquid
open System.Collections.Generic

type View(viewConfig : ViewConfig, viewPathFinder : ViewPathFinder) =

    (*let toRenderParameters = function
    | Some (viewData : ViewData) -> 
        viewData
        |> Map.fold
            (fun (acc : RenderParameters) k v ->
                acc.LocalVariables.Add(k, v)
                acc) (new RenderParameters())
    | _ -> new RenderParameters()*)

    let toRenderParameters (viewData : ViewData option) =
        match viewData with
        | Some viewData -> viewData
        | _ -> Map.empty
        |> Map.toSeq
        |> dict
        |> Hash.FromDictionary

    interface IView with
        override v.Render(routeParams, path, viewData) =
            path
            |> function
            | Some path -> viewPathFinder.TryFind(routeParams, path)
            | _ -> viewPathFinder.TryFind(routeParams)
            |> function
            | Some path -> 
                File.ReadAllText(path, Encoding.UTF8)
                |> fun text ->
                    Template.Parse(text).Render(viewData |> toRenderParameters)
            | _ -> failwith "Template file not fonud"

    new(viewConfig : ViewConfig) =
        View(viewConfig, new ViewPathFinder(viewConfig))