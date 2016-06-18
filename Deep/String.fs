[<AutoOpen>]
module Deep.StringModule

open System

type String with

    member s.TryConvertToInt() =
        let isNumeric, n = Int32.TryParse(s)
        if isNumeric then Some n
        else None

    member s.TryConvertToDecimal() =
        let isNumeric, n = Decimal.TryParse(s)
        if isNumeric then Some n
        else None
