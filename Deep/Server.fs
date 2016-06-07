module Deep.Server

open System.Net
open System.Threading

let listen uri action =
    let listener = new HttpListener()
    listener.Prefixes.Add(uri)
    let asyncContext = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
    let rec loop () = async {
        let! context = asyncContext
        async { action context } |> Async.Start
        do! loop ()
    }
    listener.Start()
    loop () |> Async.Start