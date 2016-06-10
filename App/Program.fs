open System
open Deep
open Deep.Routing

[<Get("/?param1/?param2")>]
let hello1 (req : Request) (res : Response) =
    res.ContentType <- "text/html"
    use writer = res.Writer
    writer |> wprintf "Hello <strong>World!</strong> %s" req.Params.["param1"]

type App(kernel, router) =
    inherit HttpApplication(kernel, router)

    override a.RegisterRoutes(routes) =
        routes |> Routes.AddMarkedActions [System.Reflection.Assembly.GetExecutingAssembly()]

[<EntryPoint>]
let main argv =
    App(new WindsorKernel(), new Router()).Run("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0