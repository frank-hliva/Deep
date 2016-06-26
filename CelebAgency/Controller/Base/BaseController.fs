namespace CelebAgency

open Deep
open Deep.Mvc

type BaseController() =
    
    member c.Loaded(request : Request, reply : Reply) =
        reply.ViewData.["ActualPath"] <- "Fero Hliva"

    member c.Error404(kernel : IKernel) =
        "Error/Page404" |> Controller.executeAction kernel