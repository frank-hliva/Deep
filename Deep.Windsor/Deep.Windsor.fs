module Deep.Windsor

open Deep
open Castle.Windsor
open Deep.Routing

type WindsorResolver(container : IWindsorContainer) =
    interface IExternalResolver with
        member r.Contains(t) = container.Kernel.HasComponent(t)
        member r.Resolve(t) = container.Resolve(t)

type ApplicationBooter<'t when 't : not struct and 't :> IApplication>(container : IWindsorContainer) =
    inherit Deep.ApplicationBooter<'t>(container |> WindsorResolver |> Kernel)
    override b.Boot(uri : string) = b.Kernel.Resolve<'t>().Run(uri)
    member b.WindsorContainer = container
    member b.Config(configurator : IWindsorContainer -> IWindsorContainer) =
        base.Config(id)
        configurator(container)
        |> ignore