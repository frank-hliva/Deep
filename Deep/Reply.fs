namespace Deep

open System
open System.IO
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type Reply(request : Request, response : Response, view : IView) =
    static let printToReply (value : string) (reply : Reply) =
        reply.Writer.Write(value)
        reply
    static let printToReplyLineEnd (value : string) (reply : Reply) =
        reply.Writer.Write(value + Environment.NewLine)
        reply
    do response.ContentType <- ContentTypes.html
    do response.ContentEncoding <- Encoding.UTF8
    let writer = response.GetWriter()
    let toViewData = function
    | Some (viewData : (string * obj) list) -> viewData |> Map |> Some
    | _ -> None
    member val internal IsDisposed = false with get, set
    interface IDisposable with
        member r.Dispose() =
            writer.Dispose()
            r.IsDisposed <- true
    member r.Response = response
    member r.Writer : StreamWriter = writer
    member private r.View(path : string option, viewData : ViewData option) =
        match view with
        | null -> failwith "View engine not found"
        | _ -> view.Render(request.Params, path, viewData) |> writer.Write
    member r.View(path : string, ?viewData : ViewData) = r.View(Some path, viewData)
    member r.View(?viewData : ViewData) = r.View(None, viewData)
    member r.View(path : string, ?viewData : (string * obj) list) =
        r.View(Some path, viewData |> toViewData)
    member r.View(?viewData : (string * obj) list) =
        r.View(None, viewData |> toViewData)
    member this.StatusCode with get() = response.StatusCode and set(value) = response.StatusCode <- value
    member this.ContentEncoding with get() = response.ContentEncoding and set(value) = response.ContentEncoding <- value
    member this.ContentType with get() = response.ContentType and set(value) = response.ContentType <- value
    member r.Redirect(url : string) = response.Redirect(url)
    member r.End(?statusCode : int) =
        match statusCode with
        | Some statusCode -> r.StatusCode <- statusCode
        | _ -> ()
        (r :> IDisposable).Dispose()
    new(request : Request, response : Response) =
        new Reply(request, response, null)
    member r.Content(html : string, contentType : string) =
        r.ContentType <- contentType
        r.Writer.Write(html)
    member r.Html(html : string) =
        r.ContentType <- ContentTypes.html
        r.Writer.Write(html)
    member r.Text(text : string) =
        r.ContentType <- ContentTypes.plainText
        r.Writer.Write(text)
    member r.Xml(xml : string) =
        r.ContentType <- ContentTypes.xml
        r.Writer.Write(xml)
    member r.Json(json : string) =
        r.ContentType <- ContentTypes.json
        r.Writer.Write(json)
    member r.AsJson(o : obj) =
        o |> JsonConvert.SerializeObject |> r.Json
    static member printf format = Printf.ksprintf printToReply format
    static member printfn format = Printf.ksprintf printToReplyLineEnd format