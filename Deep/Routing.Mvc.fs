namespace Deep.Routing

open Deep

type MvcDefaults = { Controller : string; Action : string; Id : string }

type MvcRouteHandler(defaults : MvcDefaults) =
    interface IRouteHandler with
        member h.InvokeAction(container : IKernel) =
            ()