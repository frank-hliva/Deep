namespace Deep

type ListenerResult =
| End = 0
| Next = 1

type ListenerState = Map<string, obj>
type Listener = Request -> Response -> IKernel -> ListenerState -> Async<ListenerResult>
type ListenerSync = Request -> Response -> IKernel -> ListenerState -> ListenerResult

type IListener = 
    abstract Listen : Request -> Response -> IKernel -> ListenerState -> Async<ListenerResult>

type ListenerContainer(listeners : Listener list) =

    let rec loop (request : Request) (response : Response) (kernel : IKernel) (state : ListenerState) (listeners : Listener list) = async {
        match listeners with
        | (listener : Listener) :: xs -> 
            let! result = listener request response kernel state
            match result with
            | ListenerResult.Next ->
                do! xs |> loop request response kernel state
            | ListenerResult.End -> ()
            | _ -> failwith "Invalid listener result"
        | _ -> () }

    member c.Use(listener : #IListener) =
        new ListenerContainer(listeners @ [listener.Listen])

    member c.Use(listener : Listener) =
        new ListenerContainer(listeners @ [listener])

    member c.Use(listener : ListenerSync) =
        new ListenerContainer(listeners @ [fun request response kernel state -> async { return listener request response kernel state }])

    member c.Apply(kernel : IKernel) =
        let request = kernel.Resolve<Request>()
        let response = kernel.Resolve<Response>()
        listeners |> loop request response kernel Map.empty

    new() = ListenerContainer(List.empty)
