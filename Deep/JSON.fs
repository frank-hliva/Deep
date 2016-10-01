namespace Deep

open Newtonsoft.Json
open Newtonsoft.Json.Converters

type JSON() =
    static member stringify (o : obj) = o |> JsonConvert.SerializeObject
    static member parse (json : string) = json |> JsonConvert.DeserializeObject