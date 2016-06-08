open System
open System.IO

let private printToWriter s (writer : TextWriter) =
    writer.Write(s : string)
    writer

let private printToWriterLineEnd s (writer : TextWriter) =
    writer.Write(s + Environment.NewLine)
    writer

let printf format = Printf.fprintf printToWriter format
let printfn format = Printf.ksprintf printToWriterLineEnd format


let ms = new MemoryStream()
let writer = new StreamWriter(ms)
writer.Write("xxx");
writer |> printf "%s" "fero"
writer |> printf "%s %d" "hliva" 5
writer.Flush()
//stream |> printf "%s" "fero"

ms.Position <- 0L
let reader = new StreamReader(ms)
reader.ReadToEnd()

