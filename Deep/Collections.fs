namespace Deep.Collections

module Map =

    let ofDict dictionary = 
        (dictionary :> seq<_>)
        |> Seq.map (|KeyValue|)
        |> Map.ofSeq

    let addMap (map2 : Map<_, _>) (map1 : Map<_, _>) =
        map2 |> Seq.fold(fun acc kv -> acc |> Map.add kv.Key kv.Value) map1