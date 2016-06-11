module CelebAgency.Application

open System
open Deep
open Deep.Routing
open Castle.Windsor

[<Get("/?param1/?param2")>]
let hello1 (req : Request) (res : Response) (test : Container.TestClass) =
    res.ContentType <- "text/html"
    use writer = res.Writer
    writer |> wprintf "Hello <strong>World!</strong> %s %s" req.Params.["param1"] test.Member

type App(kernel, router) =
    inherit HttpApplication(kernel, router)

    override a.RegisterRoutes(routes) =
        routes |> Routes.AddMarkedActions [System.Reflection.Assembly.GetExecutingAssembly()]

[<EntryPoint>]
let main argv =
    let booter = new ApplicationBooter<App>(new WindsorContainer())
    booter.Config(Container.config)
    booter.Boot("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0