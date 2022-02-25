// ---------------------------------
// Utils
// ---------------------------------

module Utils

open FSharp.Data
open Nager.PublicSuffix
open Giraffe.Core
open AngleSharp
open Views

let DecodeUrl (url: string) = 
    url.Replace("-", "+").Replace("_", "/")
    |> System.Convert.FromBase64String
    |> System.Text.Encoding.ASCII.GetString

let ExtractValueFromBody (body: HttpResponseBody) =
    match body with
    | Text x -> x
    | Binary x -> "Nothing to do."

let getDomain (url: string) =
    let domainParser = new DomainParser(new WebTldRuleProvider())
    let domainInfo = domainParser.Parse(url)
    domainInfo.RegistrableDomain

let displayError errorMessage =
    let model = { Text = errorMessage }
    let view = Views.errorPage model
    htmlView view

let getAMPUrl (path: string): string =
    match path.EndsWith("/amp") with
    | true -> path
    | _ -> 
        match path.EndsWith("/amp/") with
        | true -> path
        | _ ->
            match path.EndsWith("/") with
            | true -> path + "amp"
            | _ -> path + "/amp"

let getHTMLDoc (htmlContent: string) = 
    let cfg = Configuration.Default.WithDefaultLoader()
    let ctx = BrowsingContext.New(cfg)
    async { return! ctx.OpenAsync(fun req -> req.Content(htmlContent) |> ignore) |> Async.AwaitTask } |> Async.RunSynchronously