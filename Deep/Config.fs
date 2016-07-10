namespace Deep

open System
open System.IO
open Deep.IO
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Reflection

type IConfigSource =
    abstract ToJObject : unit -> JObject

type ConfigTextSource(text : string) =
    let jObject = text |> JObject.Parse
    interface IConfigSource with
        override s.ToJObject() = jObject

type ConfigFileSource(path : string) =
    let jObject =
        File.ReadAllText(path, Encoding.UTF8)
        |> JObject.Parse
    interface IConfigSource with
        override s.ToJObject() = jObject

type Config(source : IConfigSource) =
    member c.Config = source.ToJObject()
    member c.SelectAs<'t>(path : string) =
        JsonConvert.DeserializeObject<'t>(source.ToJObject().SelectToken(path).ToString())
    new(path : string) = Config(path |> ConfigFileSource)
    new() = Config(Path.join([AppDomain.CurrentDomain.BaseDirectory; "../../App.json"]))

type IConfigSection = interface end

type IAssemblyConfig =
    abstract GetAssemblies: unit -> Assembly[]

[<AbstractClass>]
type AssemblyConfig() =

    abstract GetAssemblyNames : unit -> string[]

    member c.GetAsseblyNameSet() = c.GetAssemblyNames() |> Set.ofArray

    interface IAssemblyConfig with
        override c.GetAssemblies() =
            let set = c.GetAsseblyNameSet()
            AppDomain.CurrentDomain.GetAssemblies()
            |> Array.filter(fun a -> a.FullName |> set.Contains)

type AppInfo = { Name : string; Email : string; InfoEmail : string; SupportEmail : string }

type AppInfoConfig(config : Config) =
    interface IConfigSection
    member c.GetAppInfo() = config.SelectAs<AppInfo>("AppInfo")

type ServerOptions = { UriPrefix : string }

type ServerConfig(config : Config) =
    interface IConfigSection
    member c.GetServerOptions() = config.SelectAs<ServerOptions>("ServerOptions")