namespace Deep.Collections

open System.Collections.Generic

module Map =
    
    let ofDict dictionary = 
        (dictionary :> seq<_>)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let toDict (map : Map<'a, 'b>) = map :> IDictionary<'a, 'b>

    let addMap (map2 : Map<_, _>) (map1 : Map<_, _>) =
        map2 |> Seq.fold(fun acc kv -> acc |> Map.add kv.Key kv.Value) map1