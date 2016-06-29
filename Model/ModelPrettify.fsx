open System
open System.IO
open System.Xml
open System.Text

let isElement (name : string) (line : string) =
    let line = line.Trim()
    line.StartsWith(sprintf "<%s " name) && line.EndsWith ">"

let private toAttr = sprintf " %s=\"%s\""

let private hasAttrWithValue (name : string) (value : string) (line : string) =
    line.Contains(toAttr name value)

let private isIdentChar (c : char) = Char.IsLetterOrDigit(c) || c = '_' || c = '-'

let getAttr (name : string) (line : string) =
    let rec getAttr (name : string) (startIndex : int) (line : string) =
        let beginPattern = name + "=\""
        match line.IndexOf(beginPattern, startIndex + 1) with
        | -1 -> line
        | startIndex when line.[startIndex - 1] |> isIdentChar |> not ->
            let valueBegin = startIndex + beginPattern.Length
            let valueEnd = line.IndexOf("\"", valueBegin + 1)
            line.[valueBegin..valueEnd - 1]
        | startIndex -> line |> getAttr name (startIndex + beginPattern.Length + 1)
    line |> getAttr name 0

let private setAttr (name : string) (newValue : string) (line : string) =
    let rec setAttr (name : string) (newValue : string) (startIndex : int) (line : string) =
        let beginPattern = name + "=\""
        match line.IndexOf(beginPattern, startIndex + 1) with
        | -1 -> line
        | startIndex when line.[startIndex - 1] |> isIdentChar |> not ->
            let valueBegin = startIndex + beginPattern.Length
            let valueEnd = line.IndexOf("\"", valueBegin + 1)
            line.[..valueBegin - 1] + newValue + line.[valueEnd..]
        | startIndex -> line |> setAttr name newValue (startIndex + beginPattern.Length + 1)
    line |> setAttr name newValue 0

let private replacePropLine (``from`` : string) (``to`` : string) line =
    if line |> isElement "NavigationProperty" && line |> hasAttrWithValue "Relationship" ``from`` then
        line |> setAttr "Name" ``to``
    else line

let private changePropLineBy (changes : (string * string * string) seq) line =
    changes
    |> Seq.fold
        (fun acc (from', _, to') ->
            acc |> replacePropLine from' to') line
                   
let change (changes : Map<string, (string * string * string) list>) edmxFilePath =
    edmxFilePath
    |> File.ReadAllLines 
    |> Seq.fold
        (fun (entityType, lines) line ->
            if line |> isElement "EntityType" then
                line |> getAttr "Name", line :: lines
            else     
                match changes |> Map.tryFind entityType with
                | Some change -> entityType, (line |> changePropLineBy change) :: lines
                | _ -> entityType, line :: lines) ("", List.empty)
    |> fun (_, lines) -> File.WriteAllLines(edmxFilePath, lines |> List.rev)

let changes =
    Map [
        "Comment", [
            "Self.FK_Comments_AuthorUser", "User", "AuthorUser"
            "Self.FK_Comments_CommentedUser", "User1", "CommentedUser"
        ]
        "User", [
            "Self.FK_Comments_AuthorUser", "Comments", "AuthorComments"
            "Self.FK_Comments_CommentedUser", "Comments1", "Comments"
            "Self.FK_Likes_AuthorUser", "Likes", "AuthorLikes"
            "Self.FK_Likes_TargetUser", "Likes1", "Likes"
        ]
        "Like", [
            "Self.FK_Likes_AuthorUser", "Likes", "AuthorUser"
            "Self.FK_Likes_TargetUser", "Likes1", "LikedUser"
        ]
    ]

Path.Combine(__SOURCE_DIRECTORY__ , "Model.edmx") |> change changes

changes
|> Map.iter
    (fun myClass changes ->
        let path = Path.Combine(__SOURCE_DIRECTORY__ , sprintf "%s.cs" myClass)
        path
        |> File.ReadAllText
        |> fun content -> 
            changes
            |> List.fold
                (fun (acc : StringBuilder) (_, from', to') ->
                    acc.Replace(from', to')) (new StringBuilder(content))
        |> fun out -> File.WriteAllText(path, out.ToString()))