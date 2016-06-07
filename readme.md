# Deep

F# MVC Web framework version 1.0.0

## License:

https://github.com/frank-hliva/Deep/blob/master/LICENSE.md

## Example:

```fs
open System.Net
open System.Text
open System.IO
open Deep
open Deep.Routing

[<Get("/?controller/?action")>]
let controllerHandler (outputStream : StreamWriter) (res : HttpListenerResponse) =
    res.ContentType <- "text/html"
    use stream = outputStream
    stream.WriteLine("Hello <strong>World!</strong>")
    |> ignore

type App() =
    inherit HttpApplication()

    override a.RegisterRoutes(routes : routes) =
        routes |> RouteFinder.findMarkedFunctions [System.Reflection.Assembly.GetExecutingAssembly()]

[<EntryPoint>]
let main argv =
    App().Run("http://127.0.0.1:3000/")
    System.Console.WriteLine("Server running on port 3000...")
    System.Console.ReadLine() |> ignore
    0
```