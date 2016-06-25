[<AutoOpen>]
module Deep.Writer

open System
open System.IO
open System.IO.Compression

let private printToWriter (value : string) (writer : TextWriter) =
    writer.Write(value)
    writer

let private printToWriterLineEnd (value : string) (writer : TextWriter) =
    writer.Write(value + Environment.NewLine)
    writer

let wprintf format = Printf.ksprintf printToWriter format
let wprintfn format = Printf.ksprintf printToWriterLineEnd format

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
    | encoding -> encoding, response.OutputStream
    |> fun (encoding, stream) ->
        response.Headers.Add("Content-Encoding", encoding)
        new StreamWriter(stream)