module App.Application

open Deep
open Deep.Routing
open Deep.Windsor
open Castle.Windsor
open System

type HomeController(reply : Reply) =

    member c.Index() =
        reply.View([ "name" => "Frank Hliva" ])

    member c.Page(id : int) =
        reply |> Reply.printf "Fero %d" id

    member c.Download() =
        reply.SendFile(@"img/1397648551.jpg", "xxx.jpeg")

[<Get("/test/?param1/?param2")>]
let hello1 (req : Request) (reply : Reply) =
    reply |> Reply.printf "Hello <strong>World!</strong> %s" req.Params.["param1"]

[<EntryPoint>]
let main argv =
    let booter = new ApplicationBooter<HttpApplication>(new WindsorContainer())
    booter.Config(config)
    booter.Boot("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0