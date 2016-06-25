namespace Deep

open System.IO
open System.Web
open Deep.IO

type StaticContentOptions = { Directory : string }

type StaticContentConfig(config : Config) =
    member c.GetOptions() =
        let options = config.SelectAs<StaticContentOptions>("StaticContent")
        { options with Directory = options.Directory |> Path.map }

type StaticContent(staticContentOptions : StaticContentOptions) =

    let sendFile (response : Response) (path : string) = async {
            use fileStream = File.OpenRead(path)
            let fileName = Path.GetFileName(path)
            response.ContentLength64 <- fileStream.Length
            response.SendChunked <- false
            response.ContentType <- MimeMapping.GetMimeMapping(fileName)
            let buffer = Array.create(64 * 1024) 0uy
            let rec loop () = async {
                let! read = fileStream.AsyncRead(buffer, 0, buffer.Length)
                if read > 0 then
                    do! response.OutputStream.AsyncWrite(buffer, 0, read)
                    do! loop() }
            do! loop()
        }

    interface IListener with

        member l.Listen (request : Request) (response : Response) (kernel : IKernel) (state : ListenerState) = async {
            if request.RawUrl = "/" then return ListenerResult.Next
            else
                let path = Path.join([staticContentOptions.Directory; request.RawUrl])
                if File.Exists(path) then
                    do! path |> sendFile response
                    return ListenerResult.End
                else return ListenerResult.Next }

    new(config : StaticContentConfig) =
        StaticContent(config.GetOptions())