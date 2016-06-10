module CelebAgency.Container

open Deep
open Castle.Windsor

let fromKernel (kernel : IKernel) = (kernel :?> WindsorKernel).Container

let config (container : IWindsorContainer) =
    ()