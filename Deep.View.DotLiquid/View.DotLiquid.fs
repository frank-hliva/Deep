namespace Deep.View.DotLiquid

open Deep
open System.IO
open System.Text
open DotLiquid
open System.Collections.Generic
open Deep.Routing
open Deep.Collections
open Microsoft.FSharp.Reflection

type View(viewConfig : ViewConfig, viewPathFinder : ViewPathFinder) =

    let rec toHash (values : obj) =
        match values with
        | :? IDictionary<string, obj> as dict ->
            dict
            |> Seq.map(fun kv -> kv.Key, kv.Value |> box |> toHash)
            |> Map.ofSeq
            :> IDictionary<string, obj>
            |> Hash.FromDictionary
            |> box
        | :? seq<string * obj> as s -> s |> Map.ofSeq |> toHash
        | :? string -> values
        | value when value.GetType().IsValueType -> values
        | record when record.GetType().IsClass ->
            Hash.FromAnonymousObject(record) |> box
        | _ -> failwith "Invalid type"

    let toRenderParameters (viewData : ViewData option) =
        match viewData with
        | Some viewData -> viewData
        | _ -> Map.empty
        |> Map.toDict
        |> toHash :?> Hash

    let readFromFile (routeParams : RouteParams) (path : string option) =
        path
        |> function
        | Some path -> viewPathFinder.TryFind(routeParams, path)
        | _ -> viewPathFinder.TryFind(routeParams)
        |> function
        | Some path -> File.ReadAllText(path, Encoding.UTF8)
        | _ -> failwith "Template file not fonud"

    let localFileSystem (routeParams : RouteParams) =
        { new DotLiquid.FileSystems.IFileSystem with
            member this.ReadTemplateFile(context, name) =
                name.Trim([|'\"';'\''|]).Trim()
                |> Some |> readFromFile routeParams }

    interface IView with
        override v.Render(routeParams, path, viewData) =
            let text = path |> readFromFile routeParams
            let o = new obj()
            lock o (fun () -> Template.FileSystem <- localFileSystem routeParams)
            Template.Parse(text).Render(viewData |> toRenderParameters)

    new(viewConfig : ViewConfig) =
        View(viewConfig, new ViewPathFinder(viewConfig))