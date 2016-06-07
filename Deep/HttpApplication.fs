namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

type [<AbstractClass>] HttpApplication() =

    let routes = []

    let getActionParams (context : HttpListenerContext) (matchResult : MatchResult) : obj list =
        [
            fun () -> context;
            fun () -> matchResult.Parameters
            fun () -> context.Request
            fun () -> context.Response
            fun () -> new StreamReader(context.Request.InputStream)
            fun () -> new StreamWriter(context.Response.OutputStream)
        ]

    abstract RegisterRoutes : routes -> routes

    member a.RegisterActionParams (context : HttpListenerContext) (matchResult : MatchResult) (actionParams : obj list) =
        actionParams

    member a.ProccessResult (context : HttpListenerContext) (matchResult : MatchResult option) =
        match matchResult with
        | Some result ->
            let actionParams =
                getActionParams context result
                |> a.RegisterActionParams context result
            result.Handler.InvokeAction(actionParams)
        | _ -> ()

    member a.Listener(context : HttpListenerContext) =
        let req = context.Request
        a.RegisterRoutes(routes)
        |> Routes.match' req.HttpMethod req.RawUrl
        |> a.ProccessResult context

    member a.Run(uri : string) =
        Server.listen uri a.Listener