namespace Deep

open Deep
open Deep.Mvc

type ErrorHandler() =

    let toHttpStatusCode (e : exn) =
        match e with
        | :? HttpException as e ->
            match e.Data0 with
            | 403 | 404 | 500 -> e.Data0
            | _ -> 500
        | _ -> 500

    interface IListener with

        member r.Listen (request : Request) (response : Response) (kernel : IKernel) (e : exn option) = async {
            match e with
            | Some e ->
                //response.Redirect(sprintf "/error/page%d" (e |> toHttpStatusCode))
                let action = sprintf "Error/Page%d" (e |> toHttpStatusCode)
                do! action |> Controller.executeAction kernel
                kernel |> AutoDisposer.disposeObjects
                response.Close()
                return ListenerResult.End 
            | _ -> return ListenerResult.Next }