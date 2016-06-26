namespace CelebAgency

open Deep

type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index() =
        reply.ViewData.["zzz"] <- "3"
        reply.View ["name" => "Fero"]
        

    member c.Page(id : int) =
        reply |> Reply.printf "Test %d" id

    member c.Download() =
        reply.SendFile(@"img/1397648551.jpg", "xxx.jpeg")