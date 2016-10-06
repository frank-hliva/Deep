namespace Deep.IO

open System
open System.IO
open System.IO.Compression
open System.Text
open Deep

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

type SendFileOptions = { BufferSize : int; ContentType : string } 

type File() =

    static let defaultSendOptions (fileName : string) (options : SendFileOptions option) =
        let defaultBufferSize = 256 * 1024
        let options =
            match options with
            | Some options -> options
            | _ -> { BufferSize = defaultBufferSize; ContentType = null }
        let options =
            if String.IsNullOrEmpty(options.ContentType)
            then { options with ContentType = MimeMapping.getMimeMapping(fileName) }
            else options
        match options.BufferSize with
        | 0 -> { options with BufferSize = defaultBufferSize }
        | _ -> options

    static member sendBinary (path : string, response : Response, ?options: SendFileOptions) = async {
        let options = options |> defaultSendOptions (Path.GetFileName path)
        use fileStream = File.OpenRead(path)
        if fileStream.Length = 0L then response.Close()
        else
            let outputStream = response.RawOutputStream
            response.ContentLength64 <- fileStream.Length
            response.SendChunked <- false
            response.ContentType <- options.ContentType
            if (response.Headers.["Cache-Control"] = null) then
                response.Headers.Add("Cache-Control", "public, max-age=31536000")
            let buffer = Array.create(options.BufferSize) 0uy
            let rec loop () = async {
                let! read = fileStream.AsyncRead(buffer, 0, buffer.Length)
                if read > 0 then
                    let! _ = outputStream.AsyncWrite(buffer, 0, read) |> Async.Catch
                    do! loop() }
            do! loop() }

    static member sendText(path : string, response : Response, ?options: SendFileOptions) = async {
        let options = options |> defaultSendOptions (Path.GetFileName path)
        response.ContentType <- options.ContentType
        response.ContentEncoding <- Encoding.UTF8
        if (response.Headers.["Cache-Control"] = null) then
            response.Headers.Add("Cache-Control", "public, max-age=31536000")
        use writer = response.GetWriter()
        writer.Write(File.ReadAllText(path))
        response.Headers |> Headers.addCharset response }

    static member send(path : string, response : Response, ?options: SendFileOptions) =
        if path |> Text.isTextFile && FileInfo(path).Length < 5000000L
        then
            if options.IsSome then File.sendText(path, response, options.Value)
            else File.sendText(path, response)
        else
            if options.IsSome then File.sendBinary(path, response, options.Value)
            else File.sendBinary(path, response)