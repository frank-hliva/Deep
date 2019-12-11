[<AutoOpen>]
module Deep.AsyncExtensions

open System.Threading.Tasks
open Microsoft.FSharp

type Control.AsyncBuilder with
    member async.Bind(t : Task<'T>, f : 'T -> Async<'R>) : Async<'R> = 
        async.Bind(Async.AwaitTask t, f)