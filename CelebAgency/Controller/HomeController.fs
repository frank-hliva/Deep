namespace CelebAgency

open Deep

type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index() =
        reply.ViewData.["zzz"] <- "3"
        reply.View ["name" => "Fero"]

    [<Get>]
    member c.Index(request : Request) =
        reply.ViewData.["zzz"] <- "GET"
        reply.View ["name" => "Fero"]
        
    [<Post>]
    member c.Page(id : int) =
        reply |> Reply.printf "Test %d" id

    [<Any>]
    member c.Download() =
        reply.SendFile(@"img/1397648551.jpg", "xxx.jpeg")