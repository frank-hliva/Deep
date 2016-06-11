module Deep.Function

open System
open System.Reflection
open Castle.Windsor

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

let getAttribute<'t> (m : MethodInfo) =
    m.GetCustomAttributes(typedefof<'t>, false) |> Seq.head :?> 't

let tryGetAttribute<'t> (m : MethodInfo) =
    let attrs = m.GetCustomAttributes(typedefof<'t>, false)
    match attrs.Length with
    | 0 -> None
    | _ -> Some(attrs |> Seq.head :?> 't)

module private Creator =

    let returnType creator =
        let args = creator.GetType().BaseType.GenericTypeArguments
        args.[args.Length - 1]

    let invoke creator =
        (creator |> getMethodInfo |> fst).Invoke(creator, [| null |])

let invoke (container : IWindsorContainer) func =
    let methodInfo, parameterInfos = 
        match box func with
        | :? MethodInfo as methodInfo ->
            methodInfo, methodInfo.GetParameters()
        | _ -> func |> getMethodInfo
    let toParam (paramInfo : ParameterInfo) =
        container.Resolve(paramInfo.ParameterType)
    methodInfo.Invoke(func, parameterInfos |> Array.map toParam)

(*let invoke (paramCreators : obj list) func =
    let methodInfo, parameterInfos = 
        match box func with
        | :? MethodInfo as methodInfo ->
            methodInfo, methodInfo.GetParameters()
        | _ -> func |> getMethodInfo
    let creatorMap =
        paramCreators
        |> List.map(fun c -> (c |> Creator.returnType).GUID, c)
        |> Map |> Map.add typedefof<unit>.GUID ((fun () -> null) |> box)
    let paramChooser (paramInfo : ParameterInfo) (guid : Guid) (creator : obj) =
        if guid = paramInfo.ParameterType.GUID
        then Some(creator |> Creator.invoke)
        else None
    let toParam (paramInfo : ParameterInfo) =
        match creatorMap |> Map.tryPick(paramChooser paramInfo) with
        | Some param -> param 
        | _ -> failwith "Invalid parameter"
    methodInfo.Invoke(func, parameterInfos |> Array.map toParam)*)