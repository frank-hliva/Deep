# Deep

F# MVC Web framework version 1.0.0

## License:

https://github.com/frank-hliva/Deep/blob/master/LICENSE.md

## Example:

```fs
open System
open System.Text
open System.IO
open Deep
open Deep.Routing

[<Get("/?param1/?param2")>]
let hello (req : Request) (res : Response) =
    res.ContentType <- "text/html"
    use writer = res.Writer
    writer.WriteLine("Hello <strong>World!</strong> " + req.Params.["param1"])
    |> ignore

type App() =
    inherit HttpApplication()

    override a.RegisterRoutes(routes : routes) =
        routes |> RouteFinder.findMarkedFunctions [System.Reflection.Assembly.GetExecutingAssembly()]

[<EntryPoint>]
let main argv =
    App().Run("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0
```