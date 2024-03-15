module Deep.MimeMapping

open System.IO
open System.Web
open HeyRed.Mime;

let getMimeMapping (fileName : string) =
    match Path.GetExtension(fileName).ToLower() with
    | ".svg" -> "image/svg+xml"
    | ".webp" -> "image/webp"
    | _ -> MimeTypesMap.GetMimeType(fileName)