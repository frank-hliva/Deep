open System
open System.Reflection
open System.Collections.Generic

let isEnumerable (o : obj) =
    o.GetType().GetInterfaces()
    |> Array.exists
        (fun i ->
            i.IsGenericType &&
            i.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)

[
    [1,2;2,2;3,2] |> box
    [|1;2;3|] |> box
    seq { yield 1; yield 2 } |> box
    seq [|1..3|] |> box
    seq [1..3] |> box
] |> List.map(isEnumerable)

[1,2;2,2;3,2] |> box |> isCollection