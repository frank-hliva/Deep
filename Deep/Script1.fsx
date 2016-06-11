#r @"c:\Projekty\Deep\Deep.WindsorKernel\bin\Release\Castle.Core.dll"
#r @"c:\Projekty\Deep\Deep.WindsorKernel\bin\Release\Castle.Windsor.dll"
#r "Microsoft.VisualStudio.QualityTools.UnitTestFramework.dll"

open System
open Castle.Windsor
open Castle.MicroKernel.Registration

type IService =
    abstract Name : string

type Service1(name : string) =
    interface IService with
        member s.Name = name

(*type Service2() =
    interface IService with
        member s.Name = "Linda"

let container = new WindsorContainer()

type InstanceFactory() =
    member val Instance = null with get, set
    member f.FactoryMethod() = f.Instance

type InstanceReplacer(container : IWindsorContainer) =

    

    member r.RegisterInstance(t : Type, instance : obj) =
        let f = new InstanceFactory()
        f.Instance <- instance
        container.Register(Component
            .For(t)
            .UsingFactoryMethod(f.FactoryMethod)
            .LifeStyle.Transient)*)

let container = new WindsorContainer()

let test() =
    use child = new WindsorContainer()
    child.a