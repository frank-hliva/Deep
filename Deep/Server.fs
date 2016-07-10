module Deep.Server

open System
open System.Net
open System.Threading

let listen uriPrefix action =
    let listener = new HttpListener()
    listener.IgnoreWriteExceptions <- true
    listener.Prefixes.Add(uriPrefix)
    listener.Start()
    let rec loop () = async {
        let! context = Async.FromBeginEnd(listener.BeginGetContext, listener.EndGetContext)
        action context |> Async.Start
        do! loop ()
    }
    loop () |> Async.Start