[<AutoOpen>]
module Config

open Deep
open Castle.Windsor
open Castle.MicroKernel.Registration

let config (container : IWindsorContainer) =
    container