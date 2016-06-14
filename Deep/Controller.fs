namespace Deep.Mvc

open Deep
open System.Net
open System.Reflection

[<AllowNullLiteral>]
type internal ControllerContext(httpListenerContext : HttpListenerContext, request : Request, response : Response) =
    member c.HttpListenerContext = httpListenerContext
    member c.Request = request
    member c.Response = request
    
type Controller() =
    let controllerContext : ControllerContext = null
    member internal c.ControllerContext = controllerContext
    member c.Request = controllerContext.Request
    member c.Response = controllerContext.Response

module internal ControllerModule =
    let setControllerContext (context : ControllerContext) (controller : Controller) =
        typedefof<Controller>
           .GetField("controllerContext", BindingFlags.Instance ||| BindingFlags.NonPublic)
           .SetValue(controller, context)