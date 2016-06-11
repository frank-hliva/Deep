#r @"c:\Projekty\Deep\Deep\bin\Release\Castle.Core.dll"
#r @"c:\Projekty\Deep\Deep\bin\Release\Castle.Windsor.dll"

open Castle.Windsor
open Castle.MicroKernel.Registration

type Test() =
    member t.Name = "Fero Hliva"

let appC = new WindsorContainer()
appC.Register(Component.For(typedefof<Test>).ImplementedBy<Test>().LifeStyle.Singleton)

let childC1 = new WindsorContainer()
let childC2 = new WindsorContainer()
appC.AddChildContainer(childC1)
appC.RemoveChildContainer(childC1)
appC.AddChildContainer(childC2)

childC2.Resolve<Test>()