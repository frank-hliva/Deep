namespace CelebAgency

open Deep

type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index(sessions : ISessionManager) =
        let xxx = sessions.GetItemOrDefault<int>("key") + 1
        sessions.SetItem("key", xxx)
        let yyy = sessions.GetItem<int>("key")
        reply.ViewData.["zzz"] <- yyy
        reply.View ["name" => "Fero"]
        
    [<Post>]
    member c.Page(id : int) =
        reply |> Reply.printf "Test %d" id

    [<Any>]
    member c.Download() =
        reply.SendFile(@"img/1397648551.jpg", "xxx.jpeg")