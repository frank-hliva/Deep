namespace Deep.Imaging

open System
open System.IO
open System.Windows
open System.Windows.Media
open System.Windows.Media.Imaging

open Deep.Imaging.Sizing

module Float =
    let toInt (n : float) = n |> Math.Round |> int

module private Img =
    open System.Windows.Controls

    let toBitmapSource (width : float, height : float) (visual : DrawingVisual) =
        let target = new RenderTargetBitmap(Float.toInt width, Float.toInt height, 96.0, 96.0, PixelFormats.Pbgra32)
        target.Render(visual)
        target :> BitmapSource

    let resize (width : float, height : float) (source : BitmapSource) =
        let drawingVisual = new DrawingVisual()
        (
            use drawingContext = drawingVisual.RenderOpen()
            let group = new DrawingGroup()
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.Fant)
            group.Children.Add(new ImageDrawing(source, new Rect(0.0, 0.0, width, height)))
            drawingContext.DrawDrawing(group)
        )
        drawingVisual |> toBitmapSource (width, height)

    let newBackground (width : float, height : float) (background : Brush) (img : ImageSource option) =
        let drawingVisual = new DrawingVisual()
        (
            use drawingContext = drawingVisual.RenderOpen()
            let rect = new Rect(0.0, 0.0, width, height)
            drawingContext.DrawRectangle(background, null, rect)
            match img with
            | Some img ->
                let x = if img.Width < width then (width - img.Width) / 2.0 else 0.0
                let y = if img.Height < height then (height - img.Height) / 2.0 else 0.0
                drawingContext.DrawImage(img, Rect(x, y, img.Width, img.Height))
            | _ -> ()
        )
        drawingVisual |> toBitmapSource (width, height)

    let addImage (img2 : ImageSource) (rect : Rect) (img1 : ImageSource) =
        let drawingVisual = new DrawingVisual()
        (
            use drawingContext = drawingVisual.RenderOpen()
            drawingContext.DrawImage(img1, new Rect(0.0, 0.0, img1.Width, img1.Height))
            drawingContext.DrawImage(img2, rect)
        )
        drawingVisual |> toBitmapSource (img1.Width, img1.Height)

    let rotate (angle : float) (source : BitmapSource) =
        let transformBitmap = new TransformedBitmap()
        transformBitmap.BeginInit()
        transformBitmap.Source <- source
        transformBitmap.Transform <- new RotateTransform(angle)
        transformBitmap.EndInit()
        transformBitmap :> BitmapSource

type Image(source : BitmapSource) =
    static member Empty(width, height, background : Brush) =
        new Image(Img.newBackground (width, height) background None)
    static member From(uri : Uri) = new Image(new BitmapImage(uri))
    static member From(path : string) =
        use stream = File.OpenRead(path)
        Image.From(stream)
    static member From(stream : Stream) =
        let bitmapImage = new BitmapImage()
        bitmapImage.BeginInit()
        bitmapImage.CacheOption <- BitmapCacheOption.OnLoad
        bitmapImage.StreamSource <- stream
        bitmapImage.EndInit()
        new Image(bitmapImage)
    member i.Source = source
    member i.Map(mapper : BitmapSource -> BitmapSource) =
        new Image(i.Source |> mapper)
    member i.Resize(width, height, fit : Fit) =
        let size = Size(source.Width, source.Height)
        let ns = size.FitToArea(new Size(width, height), fit)
        new Image(source |> Img.resize (ns.Width, ns.Height))
    member i.Resize(width, height) = i.Resize(width, height, Fit.Inside)
    member i.WithBackground(width : float, height : float, background : Brush) =
        let w = if width > source.Width then width else source.Width
        let h = if height > source.Height then height else source.Height
        new Image(Img.newBackground (w, h) background (i.Source :> ImageSource |> Some))
    member i.Crop(x, y, width, height) =
        let x = if x = -1.0 && width < source.Width then (source.Width - width) / 2.0 else x
        let y = if y = -1.0 && height < source.Height then (source.Height - height) / 2.0 else y
        new Image(new CroppedBitmap(source, new Int32Rect(Float.toInt x, Float.toInt y, Float.toInt width, Float.toInt height)))
    member i.Crop(width, height) = i.Crop(-1.0, 0.0, width, height)
    member i.HasSize(width, height) =
        source.Width = width && source.Height = height
    member i.IsSmallerThan(width, height) =
        i.Width < width && i.Height < height
    member i.IsBiggerThan(width, height) =
        i.Width > width || i.Height > height
    member i.Rotate(angle : float) =
        new Image(source |> Img.rotate angle)
    member i.AddImage(img : Image, rect : Rect) =
        new Image(source |> Img.addImage img.Source rect)
    member i.AddImage(img : Image, x : float, y : float, width : float, height : float) =
        i.AddImage(img, new Rect(x, y, width, height))
    member i.AddImage(img : Image, x : float, y : float) =
        i.AddImage(img, new Rect(x, y, img.Width, img.Height))
    member i.Save(path : string, encoder : BitmapEncoder) =
        encoder.Frames.Add(BitmapFrame.Create(source))
        use filestream = new FileStream(path, FileMode.Create)
        encoder.Save(filestream)
    member i.Width = float i.Source.PixelWidth
    member i.Height = float i.Source.PixelHeight