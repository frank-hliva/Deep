namespace CelebAgency

open Deep
open Model
open System.Data.Entity
open System.Linq
open System.Threading.Tasks

type ArticleController(reply : Reply) =
    inherit FrontendController()

    member c.View(uri : string, kernel : IKernel) = async {
        use ctx = new Db()
        let! article =
            ctx.Articles.FirstAsync(fun x -> x.Uri = "about")
            |> Async.AwaitTask
        if article.IsPublished then
            c.Title <- if article.Title = "" then article.Name else article.Title
            reply.View [
                "Article" => article
                "CreationDateString" => article.CreationDate.Value.ToString("D")
                "LastUpdateString" => article.CreationDate.Value.ToString("D")
            ]
        else do! c.Error404 kernel }