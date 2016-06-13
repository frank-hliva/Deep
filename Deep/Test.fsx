#r @"c:\Projekty\Deep\CelebAgency\bin\Release\Castle.Core.dll"
#r @"c:\Projekty\Deep\CelebAgency\bin\Release\Castle.Windsor.dll"

open System
open Castle.Windsor
open Castle.MicroKernel.Registration
open Castle.MicroKernel.Context
open Castle.MicroKernel.Lifestyle
open Castle.MicroKernel
open Castle.MicroKernel.Lifestyle.Scoped
open System.Collections.Concurrent
open System.Reflection

let container = new WindsorContainer()

type LifeTime =
| PerResolve = 0
| Singleton = 1

type IExternalResolver =
    abstract Contains : Type -> bool
    abstract Resolve : Type -> obj

type WindsorResolver(container : IWindsorContainer) =
    interface IExternalResolver with
        member r.Contains(t) = container.Kernel.HasComponent(t)
        member r.Resolve(t) = container.Resolve(t)

type IKernel =
    abstract Register : Type * Type * ?lifeTime : LifeTime -> IKernel
    abstract Register : Type * ?lifeTime : LifeTime -> IKernel
    abstract Register<'t1, 't2> : ?lifeTime : LifeTime -> IKernel
    abstract Register<'t> : ?lifeTime : LifeTime -> IKernel
    abstract RegisterInstance : Type * obj -> IKernel
    abstract RegisterInstance : obj -> IKernel
    abstract RegisterInstance<'t> : obj -> IKernel
    abstract Resolve : Type -> obj
    abstract Resolve<'t> : unit -> 't
    abstract Contains : Type -> bool

type ExternalResolver(container : IKernel) =
    interface IExternalResolver with
        member r.Contains(t) = container.Contains(t)
        member r.Resolve(t) = container.Resolve(t)

type KernelItem =
    {
        Abstraction : Type
        ImplementedBy : Type option
        LifeTime : LifeTime
        mutable Instance : obj option
    }

type private KernelMap = Map<Guid, KernelItem>

type private SearchResult =
| Internal of KernelItem
| External of obj

type internal Kernel(types : KernelMap, externalResolver : IExternalResolver option) =

    let containsType (t : Type) =
        if types.ContainsKey(t.GUID) then true
        else
            match externalResolver with
            | Some resolver -> resolver.Contains(t)
            | _ -> false

    let containsParamType (p : ParameterInfo) =
        p.ParameterType |> containsType

    let find (t : Type) (types : KernelMap) =
        match types |> Map.tryFind t.GUID with
        | Some kernelItem -> Internal(kernelItem)
        | _ ->
            match externalResolver with
            | Some resolver -> External(resolver.Resolve(t))
            | _ -> failwith "Type not registered"

    let chooseParams (c : ConstructorInfo) =
        let parameters = c.GetParameters()
        if parameters |> Seq.forall(containsParamType)
        then Some(parameters)
        else None

    let rec toParam (p : ParameterInfo) =
        let t = p.ParameterType
        if types.ContainsKey(t.GUID)
        then t |> resolve
        else externalResolver.Value.Resolve(t)

    and resolve (t : Type) =
        match types |> find t with
        | External o -> o
        | Internal i when i.Instance.IsSome -> i.Instance.Value
        | Internal i ->
            let implementation = i.ImplementedBy.Value
            implementation.GetConstructors()
            |> Seq.tryPick(chooseParams)
            |> function
            | Some parameterInfos ->
                let instance = Activator.CreateInstance(implementation, parameterInfos |> Array.map(toParam))
                match i.LifeTime with
                | LifeTime.PerResolve -> instance
                | LifeTime.Singleton ->
                    i.Instance <- Some instance
                    instance
                | _ -> failwith "Invalid lifetime"
            | _ -> failwith "Constructor parameter mismatch"

    interface IKernel with

        member k.Register(abstraction : Type, implementedBy : Type, ?lifeTime : LifeTime) =
            new Kernel(types |> Map.add abstraction.GUID {
                Abstraction = abstraction
                ImplementedBy = Some implementedBy
                LifeTime = defaultArg lifeTime LifeTime.PerResolve
                Instance = None
            }, externalResolver) :> IKernel

        member k.Register(implementedBy : Type, ?lifeTime : LifeTime) =
            new Kernel(types |> Map.add implementedBy.GUID {
                Abstraction = implementedBy
                ImplementedBy = Some implementedBy
                LifeTime = defaultArg lifeTime LifeTime.PerResolve
                Instance = None
            }, externalResolver) :> IKernel

        member k.Register<'t1, 't2>(?lifeTime : LifeTime) =
            let abstraction = typedefof<'t1>
            let implementedBy = typedefof<'t2>
            new Kernel(types |> Map.add abstraction.GUID {
                Abstraction = abstraction
                ImplementedBy = Some implementedBy
                LifeTime = defaultArg lifeTime LifeTime.PerResolve
                Instance = None
            }, externalResolver) :> IKernel

        member k.Register<'t>(?lifeTime : LifeTime) =
            let implementedBy = typedefof<'t>
            new Kernel(types |> Map.add implementedBy.GUID {
                Abstraction = implementedBy
                ImplementedBy = Some implementedBy
                LifeTime = defaultArg lifeTime LifeTime.PerResolve
                Instance = None
            }, externalResolver) :> IKernel

        member k.RegisterInstance(abstraction : Type, instance : obj) =
            new Kernel(types |> Map.add abstraction.GUID {
                Abstraction = abstraction
                ImplementedBy = None
                LifeTime = LifeTime.Singleton
                Instance = Some instance
            }, externalResolver) :> IKernel

        member k.RegisterInstance<'t>(instance : obj) =
            let abstraction = typedefof<'t>
            new Kernel(types |> Map.add abstraction.GUID {
                Abstraction = abstraction
                ImplementedBy = None
                LifeTime = LifeTime.Singleton
                Instance = Some instance
            }, externalResolver) :> IKernel

        member k.RegisterInstance(instance : obj) =
            let abstraction = instance.GetType()
            new Kernel(types |> Map.add abstraction.GUID {
                Abstraction = abstraction
                ImplementedBy = None
                LifeTime = LifeTime.Singleton
                Instance = Some instance
            }, externalResolver) :> IKernel

        member k.Resolve(t : Type) = t |> resolve

        member k.Resolve<'t>() =
            (k :> IKernel).Resolve(typedefof<'t>) :?> 't

        member k.Contains(t) = t |> containsType

    new(?resolver) = Kernel(Map.empty, resolver)
    new(resolver : IKernel) = Kernel(Map.empty, Some(new ExternalResolver(resolver) :> IExternalResolver))

type Interface1 =
    abstract Name : string

type Type1() =
        member t.Name = "Fero Hliva"

type Type2(o : Type1) =
    member t.Name = o.Name

type Interface =
    abstract Name : string

type Type3(o : Type2) =
    interface Interface with
        member t.Name = o.Name

type Type4() =
    member t.Name = "Sandra Belkova"

type Type5(o : Interface, t1 : Type4) =
    member x.FullName = sprintf "%s %s" t1.Name o.Name

container.Register(Component.For<Type1>().ImplementedBy<Type1>().LifeStyle.Singleton)
container.Register(Component.For<Type2>().ImplementedBy<Type2>().LifeStyle.Singleton)

let kernel = 
    (new Kernel(WindsorResolver(container)) :> IKernel)
        //.RegisterInstance(new Type1())
        //.Register<Type2>()
        .Register(typedefof<Interface>, typedefof<Type3>)
        .Register(typedefof<Type4>)
        .Register<Type5>()

let name = kernel.Resolve<Type5>()
name.FullName



(*public class PerClientCompanyScopeAccessor : IScopeAccessor
{
    private static readonly ConcurrentDictionary<Guid, ILifetimeScope> collection = new ConcurrentDictionary<Guid, ILifetimeScope>();

    public ILifetimeScope GetScope(Castle.MicroKernel.Context.CreationContext context)
    {
        var companyID = ClientHelper.GetCurrentClientCompanyId();
        return collection.GetOrAdd(companyID, id => new ThreadSafeDefaultLifetimeScope());
    }

    public void Dispose()
    {
        foreach (var scope in collection)
        {
            scope.Value.Dispose();
        }
        collection.Clear();
    }
}*)
