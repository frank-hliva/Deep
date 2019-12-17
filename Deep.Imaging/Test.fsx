#r @"c:\Pluton\Deep\Deep\bin\Release\Deep.dll"
#r @"System.ComponentModel.DataAnnotations"
#r @"System.Numerics"
#r @"System.Transactions.dll"
#r @"WindowsBase.dll"
#r @"PresentationCore.dll"
#r @"PresentationFramework.dll"
#r @"System.Xaml.dll"
#r @"System.Drawing"
#r @"c:\Pluton\WebP.NET\WebP.NET\bin\Release\WebP.NET.dll"

#load "Imaging.Sizing.fs"
#load "Imaging.Encoding.fs"
#load "Imaging.fs"
#load "Imaging.Video.fs"

open Deep.Imaging
open Deep.Imaging.Encoding
open System.Windows.Media.Imaging
open System.IO
open System.Net
open System

let stream = File.OpenRead(@"c:\Pluton\alfa-kocky.webp")
let img = Image.From(stream)
img.Save(@"c:\Pluton\zz.webp", Encoder.UseWebP(90.0f))