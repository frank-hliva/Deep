namespace Deep

open System
open System.IO

type Reply(request : Request, response : Response, view : IView) =
    let toViewData = function
    | Some (viewData : (string * obj) list) -> viewData |> Map |> Some
    | _ -> None
    member r.Response = response
    member private r.View(path : string option, viewData : ViewData option) =
        match view with
        | null -> failwith "View engine not found"
        | _ ->
            use writer = response.Writer
            view.Render(request.Params, path, viewData)
            |> writer.Write
    member r.View(path : string, ?viewData : ViewData) = r.View(Some path, viewData)
    member r.View(?viewData : ViewData) = r.View(None, viewData)
    member r.View(path : string, ?viewData : (string * obj) list) =
        r.View(Some path, viewData |> toViewData)
    member r.View(?viewData : (string * obj) list) =
        r.View(None, viewData |> toViewData)

    new(request : Request, response : Response) = Reply(request, response, null)