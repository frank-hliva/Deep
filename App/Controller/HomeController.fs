namespace App

open Deep
open System.Linq

type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index(sessions : ISessionManager, flashMessages : FlashMessages) = async {
        do! flashMessages.Send("Flash message")
        c.Title <- "Index"
        let! counter = sessions.GetItemOrDefault<int>("counter")
        do! sessions.SetItem("counter", counter + 1)
        let! counter = sessions.GetItem<int>("counter")
        reply.ViewData.["counter"] <- counter
        reply.View ["name" => "World"]
    }