module internal Deep.AutoDisposer

open System

let disposeObjects(kernel : IKernel) =
    match kernel.TryFindInstance(typedefof<Reply>) with
    | Some reply ->
        let reply = reply :?> Reply
        if not reply.IsDisposed then (reply :> IDisposable).Dispose()
    | _ -> ()
