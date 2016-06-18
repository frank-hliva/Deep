module App.Application

open System
open Deep
open Deep.Routing
open Castle.Windsor
open Deep.Windsor
open Deep.Mvc

type HomeController() =
    inherit Controller()
    member c.Index(response : Response) =
        use writer = response.Writer
        writer |> Writer.wprintf "Test %s" "Test"

    member c.IndexFero(response : Response) =
        use writer = response.Writer
        writer |> Writer.wprintf "Fero"

(*[<Get("/?param1/?param2")>]
let hello1 (req : Request) (res : Response) =
    res.ContentType <- "text/html"
    use writer = res.Writer
    writer |> wprintf "Hello <strong>World!</strong> %s" req.Params.["param1"]*)

type App(kernel, routeBuilder, router) =
    inherit HttpApplication(kernel, routeBuilder, router)

[<EntryPoint>]
let main argv =
    let booter = new ApplicationBooter<App>(new WindsorContainer())
    booter.Config(config)
    booter.Boot("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0