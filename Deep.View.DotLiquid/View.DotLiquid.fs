namespace Deep.View.DotLiquid

open Deep
open System.IO
open System.Text
open DotLiquid
open System.Collections.Generic
open Deep.Routing
open Deep.Collections
open Microsoft.FSharp.Reflection
open System.Reflection

type View(viewConfig : ViewConfig, viewPathFinder : ViewPathFinder) =

    let rec toHash (values : obj) =
        match values with
        | null -> null
        | :? IDictionary<string, obj> as dict ->
            dict
            |> Seq.map(fun kv -> kv.Key, kv.Value |> box |> toHash)
            |> Map.ofSeq
            :> IDictionary<string, obj>
            |> Hash.FromDictionary
            |> box
        | :? seq<string * obj> as s -> s |> Map.ofSeq |> toHash
        | collection when collection |> ObjectType.isEnumerable ->
            collection |> box
        | :? string -> values
        | value when value.GetType().IsValueType -> values
        | record when FSharpType.IsRecord(record.GetType()) ->
            Hash.FromAnonymousObject(record) |> box
        | c when c.GetType().IsClass ->
            c |> objectToDict |> box |> toHash
        | _ -> failwith "Invalid type"

    and objectToDict (o : obj) =
        let props = o.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        props
        |> Seq.choose
            (fun p ->
                if p.CanRead && p.GetIndexParameters().Length = 0
                then
                    if o.GetType() = p.PropertyType then None
                    else Some(p.Name, p.GetValue(o) |> toHash)
                else None) |> Map.ofSeq |> box

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