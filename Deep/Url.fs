module Deep.Url

let removeQueryString (url : string) =
    match url.IndexOf "?" with
    | -1 -> url
    | pos -> url.[0..pos - 1]