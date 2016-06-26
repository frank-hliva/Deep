namespace CelebAgency

open System
open Deep
open Deep.Mvc

type BaseController() =
    
    member c.Loaded(reply : Reply) =
        reply.ViewData.["ActualYear"] <- DateTime.Now.Year

    member c.Error403(kernel : IKernel) =
        "Error/Page403" |> Controller.executeAction kernel

    member c.Error404(kernel : IKernel) =
        "Error/Page404" |> Controller.executeAction kernel

    member c.Error500(kernel : IKernel) =
        "Error/Page500" |> Controller.executeAction kernel

    member c.ErrorDefault(kernel : IKernel) =
        "Error/PageDefault" |> Controller.executeAction kernel