namespace Deep

open HttpUtils

type MultipartForm(request : Request) =
    let stream = request.InputStream
    let mutable isDisposed = false

    interface IAutoDisposable with
        member f.IsDisposed with get() = isDisposed
        member f.Dispose() =
            stream.Dispose()
            isDisposed <- true

    member f.Request = request
    member f.GetFields(?filePartName : string) =
        let filePartName = defaultArg filePartName ""
        let parser = new HttpMultipartParser(stream, filePartName)
        parser.Parameters