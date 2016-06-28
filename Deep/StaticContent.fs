namespace Deep

open System.IO
open System.Web
open Deep.IO

type StaticContentOptions = { Directory : string; BufferSize : int }

type StaticContentConfig(config : Config) =
    interface IConfigSection
    member c.GetOptions() =
        let options = config.SelectAs<StaticContentOptions>("StaticContent")
        { options with Directory = options.Directory |> Path.map }

type StaticContent(staticContentOptions : StaticContentOptions) =

    interface IListener with

        member l.Listen (request : Request) (response : Response) (kernel : IKernel) (state : ListenerState) = async {
            if request.RawUrl = "/" then return ListenerResult.Next
            else
                let path = Path.join([staticContentOptions.Directory; request.RawUrl])
                if File.Exists(path) then
                    do! File.send(path, response, {
                        BufferSize = staticContentOptions.BufferSize
                        ContentType = null
                    })
                    return ListenerResult.End
                else return ListenerResult.Next }

    new(config : StaticContentConfig) =
        StaticContent(config.GetOptions())