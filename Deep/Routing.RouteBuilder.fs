namespace Deep.Routing

open Deep

[<AutoOpen>]
module RouteFinder =

    open Deep
    open System.Reflection

    type Routes with

        static member AddMarkedActions (assemblies : Assembly seq) (routes : routes) =
            assemblies
            |> Function.findByAttribute<RouteAttribute>
            |> Seq.map
                (fun mi ->
                    let attr = mi |> Function.getAttribute<RouteAttribute>
                    {
                        HttpMethod = attr.HttpMethod
                        Pattern = attr.RoutePattern
                        Handler = new FunctionRouteHandler(mi) :> IRouteHandler
                        Filter = None
                        Priority = attr.Priority
                    })
            |> fun r -> routes |> Seq.append r |> List.ofSeq

type RouteBuilderConfig(config : Config) =
    inherit AssemblyConfig()
    override c.GetAssemblyConfig() =
        config.SelectAs<string[]>("RouteFinder.Assemblies")

type RouteBuilder(builder : routes -> routes, config : RouteBuilderConfig) =
    let config = config :> IAssemblyConfig
    let routes = [] |> Routes.AddMarkedActions (config.GetAssemblies()) |> builder

    interface IRouteBuilder with
        override b.Routes = routes

    new(config : RouteBuilderConfig) = RouteBuilder(id, config)