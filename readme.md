# Deep

F# MVC Web framework version 1.0.0

## License:

https://github.com/frank-hliva/Deep/blob/master/LICENSE.md

## Example:

```fs
open Deep
open Deep.Routing
open Deep.Windsor
open Castle.Windsor
open System

[<Get("/test/?param")>]
let hello (req : Request) (reply : Reply) =
    reply.Writer
    |> wprintf "Hello <strong>World!</strong> %s" req.Params.["param"]
    |> wprintf "________________________________"

[<EntryPoint>]
let main argv =
    let booter = new ApplicationBooter<HttpApplication>(new WindsorContainer())
    booter.Config(config)
    booter.Boot("http://127.0.0.1:3000/")
    Console.WriteLine("Server running on port 3000...")
    Console.ReadKey() |> ignore
    0
```