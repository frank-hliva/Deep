namespace CelebAgency

open Deep

type ErrorController() =
    inherit FrontendController()

    member c.Page403(reply : Reply) =
        reply.StatusCode <- 403
        reply.View()

    member c.Page404(reply : Reply) =
        reply.StatusCode <- 404
        reply.View()

    member c.Page500(reply : Reply) =
        reply.StatusCode <- 500
        reply.View()

    member c.PageDefault(reply : Reply) =
        reply.StatusCode <- 200
        reply.View()