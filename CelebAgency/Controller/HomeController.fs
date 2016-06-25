namespace CelebAgency

open Deep

type HomeController(reply : Reply) =

    member c.Index() =
        reply.ViewData.["xxx"] <- "yyy"
        reply.View [ "name" => "Test" ]

    member c.Page(id : int) =
        reply |> Reply.printf "Test %d" id

    member c.Download() =
        reply.SendFile(@"img/1397648551.jpg", "xxx.jpeg")