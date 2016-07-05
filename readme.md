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

## Controller example:

```fs
type HomeController(reply : Reply) =
    inherit FrontendController()

    member c.Index(flashMessages : FlashMessages) = async {
        c.Title <- "Index"
        do! flashMessages.Send("Flash message")
        reply.View ["Name" => "world"] }

    member c.LearnMore(sessions : ISessionManager) = async {
        c.Title <- "Learn more"
        let! counter = sessions.GetItemOrDefault<int>("counter")
        do! sessions.SetItem("counter", counter + 1)
        let! counter = sessions.GetItem<int>("counter")
        reply.ViewData.["counter"] <- counter
        reply.View() }
```