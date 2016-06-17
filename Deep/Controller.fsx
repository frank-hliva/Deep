#load "Deep.fs"
#load "Kernel.fs"
#load "Url.fs"
#load "Writer.fs"
#load "Function.fs"
//#load "Controller.fs"
#load "Routing.fs"
#load "Routing.Routes.fs"
#load "Routing.RouteFinder.fs"
#load "Routing.Router.fs"
#load "Http.fs"
#load "Server.fs"
#load "Boot.fs"
#load "HttpApplication.fs"

open System.Net
open Deep
open System.Reflection

[<AllowNullLiteral>]
type ControllerContext(httpListenerContext : HttpListenerContext, request : Request, response : Response) =
    member c.HttpListenerContext = httpListenerContext
    member c.Request = request
    member c.Response = request
    
type Controller() =
    let controllerContext : ControllerContext = null
    member internal c.ControllerContext = controllerContext
    member c.Request = controllerContext.Request
    member c.Response = controllerContext.Response
    
type Controller1() =
    inherit Controller()
    let controllerContext : string = "xxx"
    member c.Test1() = controllerContext

let controller = new Controller1()

let setControllerContext (context : ControllerContext) controller =
    typedefof<Controller>
       .GetField("controllerContext", BindingFlags.Instance ||| BindingFlags.NonPublic)
       .SetValue(controller, context)

#r @"c:\Projekty\Deep\Deep\bin\Release\Newtonsoft.Json.dll"
open System
open System.IO
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type Config(str : string) =
    let config = str |> JObject.Parse
    member c.Config = config
    member c.SelectAs<'t>(path : string) =
        JsonConvert.DeserializeObject<'t>(config.SelectToken(path).ToString())
    static member Load(path : string) =
        File.ReadAllText(path, Encoding.UTF8) |> Config

let config = Config.Load(@"c:\Projekty\Deep\App\Config.json")


AppDomain.CurrentDomain.GetAssemblies().[0].FullName
           //SingleOrDefault(assembly => assembly.GetName().Name == name)