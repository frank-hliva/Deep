namespace Deep

open System
open System.IO

type Reply(request : Request, response : Response, view : IView) =
    member private r.View(path : string option, viewData : ViewData option) =
        match view with
        | null -> failwith "View engine not found"
        | _ ->
            view.Render(request.Params, path, viewData)
            |> response.Writer.Write
            response.Close()
    member r.View(path : string, ?viewData : ViewData) = r.View(Some path, viewData)
    member r.View(?viewData : ViewData) = r.View(None, viewData)

    new(request : Request, response : Response) = Reply(request, response, null)