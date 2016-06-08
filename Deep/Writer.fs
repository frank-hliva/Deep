module Deep.Writer

open System
open System.IO

let private printToWriter (value : string) (writer : TextWriter) =
    writer.Write(value)
    writer

let private printToWriterLineEnd (value : string) (writer : TextWriter) =
    writer.Write(value + Environment.NewLine)
    writer

let printf format = Printf.ksprintf printToWriter format
let printfn format = Printf.ksprintf printToWriterLineEnd format