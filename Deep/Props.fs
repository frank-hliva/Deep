module Deep.Props.Object

open System
open System.Reflection
open Deep

let toSeq (o : 't) =
    o.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
    |> Seq.choose
        (fun p ->
            if p.CanRead && p.GetIndexParameters().Length = 0
            then
                if o.GetType() = p.PropertyType then None
                else
                    let value = try p.GetValue(o) with :? Exception as e -> null
                    Some(p.Name, value)
            else None)

let toMap (o : 't) =
    o |> toSeq |> Map.ofSeq

let assignSeq (o : 't, props: seq<string * obj>) =
    let objType = o.GetType()
    props
    |> Seq.iter
        (fun (key, value) ->
            let prop = objType.GetProperty(key)
            let value =
                match value with
                | null -> null
                | _ -> value |> TypeConversion.changeType prop.PropertyType
            prop.SetValue(o, value))
    o

let assignMap (o : 't, props: Map<string, obj>) =
    assignSeq(o, props |> Map.toSeq)