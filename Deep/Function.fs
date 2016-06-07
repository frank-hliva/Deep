module Deep.Function

open System
open System.Reflection

let getMethodInfo (func : obj) =
    func.GetType().GetMethods()
    |> Seq.filter(fun mi -> mi.Name = "Invoke")
    |> Seq.map(fun mi -> mi, mi.GetParameters())
    |> Seq.maxBy(fun (mi, p) -> p.Length)

let findByAttribute<'t> (assemblies : Assembly seq) =
    assemblies
    |> Seq.collect(fun a -> a.GetTypes())
    |> Seq.collect(fun t -> t.GetMethods())
    |> Seq.filter(fun m -> m.GetCustomAttributes(typedefof<'t>, false).Length > 0)

module private Creator =

    let returnType creator =
        let args = creator.GetType().BaseType.GenericTypeArguments
        args.[args.Length - 1]

    let invoke creator =
        (creator |> getMethodInfo |> fst).Invoke(creator, [| null |])

let invoke (paramCreators : obj list) func =
    let methodInfo, parameterInfos = 
        match box func with
        | :? MethodInfo as methodInfo ->
            methodInfo, methodInfo.GetParameters()
        | _ -> func |> getMethodInfo
    let creatorMap =
        paramCreators
        |> List.map(fun c -> (c |> Creator.returnType).GUID, c)
        |> Map |> Map.add typedefof<unit>.GUID ((fun () -> null) |> box)
    methodInfo.Invoke(func,
        parameterInfos
        |> Array.map
            (fun p ->
                let param =
                    creatorMap
                    |> Map.tryPick
                        (fun guid creator ->
                            if guid = p.ParameterType.GUID
                            then creator |> Creator.invoke |> Some
                            else None)
                match param with
                | Some p -> p 
                | _ -> failwith "Invalid params"))