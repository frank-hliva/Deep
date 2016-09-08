namespace Deep

open Newtonsoft.Json
open Newtonsoft.Json.Converters

type JSON() =
    static member stringify (o : obj, ignoreReferenceLoop : bool) =
        if ignoreReferenceLoop then
            let settings = 
                new JsonSerializerSettings(
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                )
            JsonConvert.SerializeObject(o, settings)
        else o |> JsonConvert.SerializeObject
    static member stringify (o : obj) = o |> JsonConvert.SerializeObject
    static member parse (json : string) = json |> JsonConvert.DeserializeObject