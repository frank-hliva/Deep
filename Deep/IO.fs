namespace Deep.IO

open System

module Path =

    let map (path : string) =
        let currentDir =
            let dir = Environment.CurrentDirectory
            if dir.EndsWith("/") then dir else sprintf "%s/" dir
        match path with
        | path when path.StartsWith("~/") ->
            sprintf "%s%s" currentDir path.[2..]
        | _ -> path
        |> fun path -> path.Replace("\\", "/")

    let join (paths : string seq) =
        paths
        |> Seq.fold
            (fun acc path ->
                match path with
                | path when acc = "" -> path
                | path when path.StartsWith("/") -> path.[1..]
                | _ -> path
                |> fun path -> System.IO.Path.Combine(acc, path)) ""
        |> map