module Deep.Windsor

open Deep
open Castle.Windsor

type WindsorResolver(container : IWindsorContainer) =
    interface IExternalResolver with
        member r.Contains(t) = container.Kernel.HasComponent(t)
        member r.Resolve(t) = container.Resolve(t)