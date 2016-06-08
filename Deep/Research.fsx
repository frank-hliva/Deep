open System.IO

let printToWriter s (writer : TextWriter) =
    writer.Write(s |> box :?> string)
    writer

let printf format = Printf.ksprintf printToWriter format


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

