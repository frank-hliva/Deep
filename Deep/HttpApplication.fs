namespace Deep

open Deep
open Deep.Routing
open System
open System.Net
open System.IO

type [<AbstractClass>] HttpApplication(router : Router) =

    let routes = []

    let getActionParams (context : HttpListenerContext) (matchResult : RouteMatchResult) : obj list =
        [
            fun () -> context
            fun () -> new Request(context.Request, matchResult.Parameters)
            fun () -> new Response(context.Response)
        ]

    abstract RegisterRoutes : routes -> routes

    member a.RegisterActionParams (context : HttpListenerContext) (matchResult : RouteMatchResult) (actionParams : obj list) =
        actionParams

    member a.ProccessResult (context : HttpListenerContext) (matchResult : RouteMatchResult option) =
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
        |> router.Match req.HttpMethod req.RawUrl
        |> a.ProccessResult context

    member a.Run(uri : string) =
        Server.listen uri a.Listener