namespace CelebAgency

open Deep
open Model
open System.Linq

type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index(sessions : ISessionManager, flashMessages : FlashMessages) = async {
        do! flashMessages.Send("test")
        use ctx = new Db()
        let article = ctx.Articles.Where(fun x -> x.Uri = "about").First()
        c.Title <- "Index"
        let! xxx = sessions.GetItemOrDefault<int>("key")
        do! sessions.SetItem("key", xxx + 1)
        let! yyy = sessions.GetItem<int>("key")
        reply.ViewData.["zzz"] <- article.Name
        reply.View ["name" => "Fero"] }