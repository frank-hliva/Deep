module Deep.Server

open System.Net
open System.Threading

let listen url action =
    let listener = new HttpListener()
    listener.Prefixes.Add(url)
    listener.Start()
    let asyncContext = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
    let rec loop () = async {
        let! context = asyncContext
        action context |> Async.Start
        do! loop ()
    }
    loop () |> Async.Start