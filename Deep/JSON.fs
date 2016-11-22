namespace Deep

open Newtonsoft.Json
open Newtonsoft.Json.Converters

type JSON() =
    static member stringify (o : obj) = o |> JsonConvert.SerializeObject
    static member parse<'t> (json : string) = JsonConvert.DeserializeObject<'t>(json)
    static member parse (json : string) = JsonConvert.DeserializeObject(json)