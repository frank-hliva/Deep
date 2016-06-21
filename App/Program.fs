module App.Application

open Deep
open Deep.Routing
open Deep.Windsor
open Castle.Windsor
open System

type HomeController(response : Response, reply : Reply) =

    member c.Index() =
        reply.View()

    member c.Page(id : int) =
        let writer = response.Writer
        writer |> Writer.wprintf "Fero %d" id

[<Get("/test/?param1/?param2")>]
let hello1 (req : Request) (res : Response) =
    res.ContentType <- "text/html"
    use writer = res.Writer
    writer |> wprintf "Hello <strong>World!</strong> %s" req.Params.["param1"]

[<EntryPoint>]
let main argv =
    let booter = new ApplicationBooter<HttpApplication>(new WindsorContainer())
    booter.Config(config)
    booter.Boot("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0