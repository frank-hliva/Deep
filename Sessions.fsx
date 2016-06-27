open System
open System.Collections.Generic
open System.Collections.Concurrent

type Sessions = IDictionary<string, obj>
type SessionsMap = ConcurrentDictionary<string, Sessions>

type ISessionStore =
    abstract TryGetItem<'t> : id : string -> key : string -> 't option
    abstract GetItem<'t> : id : string -> key : string -> 't
    abstract GetItemOrDefault<'t> : id : string -> key : string -> 't
    abstract SetItem : id : string -> key : string -> value : obj -> unit
    abstract ContainsKey : id : string -> key : string -> bool
    abstract RemoveItem : id : string -> key : string -> unit
    abstract GetItems : id : string -> Sessions

type MemorySessionStore() =
    let dict = new SessionsMap()
    let byId (id : string) (dict : SessionsMap) =
        dict.GetOrAdd(id, new ConcurrentDictionary<string, obj>())
        :?> ConcurrentDictionary<string, obj>
    interface ISessionStore with
        member s.TryGetItem<'t> id key =
            let hasValue, value = (dict |> byId id).TryGetValue(key)
            if hasValue then value :?> 't |> Some
            else None
        member s.GetItem<'t> id key = (dict |> byId id).[key] :?> 't
        member s.GetItemOrDefault<'t> id key =
            let hasValue, value = (dict |> byId id).TryGetValue(key)
            if hasValue then value :?> 't
            else Unchecked.defaultof<'t>
        member s.SetItem id key value =
            (dict |> byId id).[key] <- value
        member s.ContainsKey id key =
            (dict |> byId id).ContainsKey(key)
        member s.RemoveItem id key =
            (dict |> byId id).TryRemove(key) |> ignore
        member s.GetItems id = (dict |> byId id) :> Sessions

type ISessionManager =
    abstract TryGetItem<'t> : key : string -> 't option
    abstract GetItem<'t> : key : string -> 't
    abstract GetItemOrDefault<'t> : key : string -> 't
    abstract TryGetItem : key : string -> obj option
    abstract GetItem : key : string -> obj
    abstract GetItemOrDefault : key : string -> obj
    abstract SetItem : key : string * value : obj -> unit
    abstract ContainsKey : key : string -> bool
    abstract RemoveItem : key : string -> unit
    abstract Items : Sessions

type SessionsManager(store : ISessionStore, id : string) =
    interface ISessionManager with
        member s.TryGetItem<'t>(key) = store.TryGetItem<'t> id key
        member s.GetItem<'t>(key) = store.GetItem<'t> id key
        member s.GetItemOrDefault<'t>(key) = store.GetItemOrDefault<'t> id key
        member s.TryGetItem(key) = store.TryGetItem<obj> id key
        member s.GetItem(key) = store.GetItem<obj> id key
        member s.GetItemOrDefault(key) = store.GetItemOrDefault<obj> id key
        member s.SetItem(key, value) = store.SetItem id key value
        member s.ContainsKey(key) = store.ContainsKey id key
        member s.RemoveItem(key) = store.RemoveItem id key
        member s.Items = store.GetItems id

let sessionManager = new SessionsManager(new MemorySessionStore(), "test") :> ISessionManager
sessionManager.SetItem("xxx", "Fero")
sessionManager.SetItem("yyy", "Hliva")
sessionManager.GetItemOrDefault("yyy")
sessionManager.Items.Count