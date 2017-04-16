namespace Deep

open System
open System.IO
open Deep.IO
open System.Text
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Reflection
open System.Collections.Concurrent

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
    let concurrentDictionary = new ConcurrentDictionary<string, obj>()
    let configObject = source.ToJObject()
    member c.Config = configObject
    member c.SelectAs<'t>(path : string) =
        concurrentDictionary.GetOrAdd(path,
            fun path ->
                let jsonString = configObject.SelectToken(path).ToString()
                let deserialized = JsonConvert.DeserializeObject<'t>(jsonString)
                deserialized :> obj) :?> 't
    new(path : string) = Config(path |> ConfigFileSource)
    new() =
        let path = Path.join([AppDomain.CurrentDomain.BaseDirectory; "../../App.json"])
        Config(path)

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

type AppInfo = {
    Name : string
    Email : string
    InfoEmail : string
    SupportEmail : string
    DEV_ENV : string
}

type AppInfoConfig(config : Config) =
    interface IConfigSection
    member c.GetAppInfo() = config.SelectAs<AppInfo>("AppInfo")

type ServerOptions = { UriPrefix : string }

type ServerConfig(config : Config) =
    interface IConfigSection
    member c.GetServerOptions() = config.SelectAs<ServerOptions>("ServerOptions")