open System
open System.Reflection

let foo () = async { () }

let bar () = ()

System.Text.Encoding.UTF8.HeaderName

foo().GetType() |> isAsync