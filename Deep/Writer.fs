module Deep.Writer

open System
open System.IO

let private printToWriter s (writer : TextWriter) =
    writer.Write(s |> box :?> string)
    writer

let private printToWriterLineEnd s (writer : TextWriter) =
    writer.Write(s |> box :?> string + Environment.NewLine)
    writer

let printf format = Printf.ksprintf printToWriter format
let printfn format = Printf.ksprintf printToWriterLineEnd format