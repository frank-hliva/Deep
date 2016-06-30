namespace Deep.IO

open System
open System.IO
open System.Web
open System.IO.Compression
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

module ResponseStream =

    let private (|AcceptEncoding|_|) (input : string) =
        if String.IsNullOrEmpty input then None
        else ["deflate"; "gzip"] |> List.tryFind(fun e -> input.Contains e)

    let get (acceptEncoding : string) (response : Response) =
        match acceptEncoding with
        | AcceptEncoding encoding ->
            encoding,
            match encoding with
            | "deflate" -> new DeflateStream(response.OutputStream, CompressionMode.Compress, false) :> Stream
            | _ -> new GZipStream(response.OutputStream, CompressionMode.Compress, false) :> Stream
        | _ -> "none", response.OutputStream
        |> fun (encoding, stream) ->
            response.Headers.Add("Content-Encoding", encoding)
            new StreamWriter(stream)

type File() =

    static let defaultSendOptions (fileName : string) (options : SendFileOptions option) =
        let defaultBufferSize = 256 * 1024
        let options =
            match options with
            | Some options -> options
            | _ -> { BufferSize = defaultBufferSize; ContentType = null }
        let options =
            if String.IsNullOrEmpty(options.ContentType)
            then { options with ContentType = MimeMapping.GetMimeMapping(fileName) }
            else options
        match options.BufferSize with
        | 0 -> { options with BufferSize = defaultBufferSize }
        | _ -> options

    static member send (path : string, response : Response, ?options: SendFileOptions) = async {
        let options = options |> defaultSendOptions (Path.GetFileName path)
        use fileStream = File.OpenRead(path)
        if fileStream.Length = 0L then response.Close()
        else
            let outputStream = response.OutputStream
            response.ContentLength64 <- fileStream.Length
            response.SendChunked <- false
            response.ContentType <- options.ContentType
            let buffer = Array.create(options.BufferSize) 0uy
            let rec loop () = async {
                let! read = fileStream.AsyncRead(buffer, 0, buffer.Length)
                if read > 0 then
                    let! _ = outputStream.AsyncWrite(buffer, 0, read) |> Async.Catch
                    do! loop() }
            do! loop() }