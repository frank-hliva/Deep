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

    static let defaultSendOptions (mimeType : string) (options : SendFileOptions option) =
        let defaultBufferSize = 256 * 1024
        let options =
            match options with
            | Some options -> options
            | _ -> { BufferSize = defaultBufferSize; ContentType = null }
        let options =
            if String.IsNullOrEmpty(options.ContentType)
            then { options with ContentType = mimeType }
            else options
        match options.BufferSize with
        | 0 -> { options with BufferSize = defaultBufferSize }
        | _ -> options
   
    static let toMimeType = Path.GetFileName >> MimeMapping.getMimeMapping

    static member sendBinary (stream : Stream, mimeType : string, response : Response, ?options: SendFileOptions) = async {
        let options = options |> defaultSendOptions mimeType
        if stream.Length = 0L then response.Close()
        else
            let outputStream = response.RawOutputStream
            response.ContentLength64 <- stream.Length
            response.SendChunked <- false
            response.ContentType <- options.ContentType
            if (response.Headers.["Cache-Control"] = null) then
                response.Headers.Add("Cache-Control", "public, max-age=31536000")
            let buffer = Array.create(options.BufferSize) 0uy
            let rec loop () = async {
                let! read = stream.AsyncRead(buffer, 0, buffer.Length)
                if read > 0 then
                    let! _ = outputStream.AsyncWrite(buffer, 0, read) |> Async.Catch
                    do! loop() }
            do! loop() }

    static member sendBinary (path : string, response : Response, ?options: SendFileOptions) = async {
        use fileStream = File.OpenRead(path)
        let mimeType = path |> toMimeType
        return!
            match options with
            | Some options ->
                File.sendBinary(fileStream, mimeType, response, options)
            | _ ->
                File.sendBinary(fileStream, mimeType, response)
    }

    static member sendText(stream : Stream, mimeType : string, response : Response, ?options: SendFileOptions) = async {
        let options = options |> defaultSendOptions mimeType
        response.ContentType <- options.ContentType
        response.ContentEncoding <- Encoding.UTF8
        if (response.Headers.["Cache-Control"] = null) then
            response.Headers.Add("Cache-Control", "public, max-age=31536000")
        use writer = response.GetWriter()
        use reader = new StreamReader(stream)
        writer.Write(reader.ReadToEnd())
        response.Headers |> Headers.addCharset response }

    static member sendText(path : string, response : Response, ?options: SendFileOptions) = async {
        use fileStream = File.OpenRead(path)
        let mimeType = path |> toMimeType
        return!
            match options with
            | Some options ->
                File.sendText(fileStream, mimeType, response, options)
            | _ ->
                File.sendText(fileStream, mimeType, response)
    }

    static member send(path : string, response : Response, ?options: SendFileOptions) =
        if path |> Text.isTextFile && FileInfo(path).Length < 5000000L
        then
            if options.IsSome then File.sendText(path, response, options.Value)
            else File.sendText(path, response)
        else
            if options.IsSome then File.sendBinary(path, response, options.Value)
            else File.sendBinary(path, response)