// ---------------------------------
// Bypass
// ---------------------------------

module Bypass

open FSharp.Data
open Giraffe.Core
open Utils
open Views

let getArticleWithGoogle path =
    try
        let response = Http.Request(path,
            headers = [
                "User-Agent", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                "X-Forwarded-For", "66.249.66.1";
                "Referer", "google.com"
            ],
            responseEncodingOverride = "utf-8"
        )
        let model   = { Text = ExtractValueFromBody response.Body }
        let view    = Views.article model
        htmlView view
    with
    | :? System.Net.WebException as ex ->
        displayError $"Something went wrong when loading the article: {ex.InnerException.Message}"
    | Failure ex ->
        displayError $"Caught an exception: {ex}"

let getArticleLaNazione path =
    let AMPUrl = getAMPUrl path

    try
        // let response_std = Http.Request(path) #TODO: get original page and substitute article from amp
        let response_amp = Http.Request(AMPUrl, responseEncodingOverride = "utf-8")

        // let html_std = ExtractValueFromBody response_std.Body #TODO: get original page and substitute article from amp
        let html_amp = ExtractValueFromBody response_amp.Body
        
        // let document_std = getHTMLDoc html_std #TODO: get original page and substitute article from amp
        let document_amp = getHTMLDoc html_amp

        (document_amp.GetElementsByClassName("article-text")[0]).Remove()
        (document_amp.GetElementsByClassName("article-text")[0]).SetAttribute("class", "article-text")
        (document_amp.GetElementsByClassName("article-text")[0]).RemoveAttribute("itemprop") |> ignore
        (document_amp.GetElementsByClassName("article-text")[0]).RemoveAttribute("amp-access") |> ignore
        (document_amp.GetElementsByClassName("article-text")[0]).RemoveAttribute("amp-access-hide") |> ignore
        document_amp.GetElementById("piano-modal-subscribe").Remove()
        (document_amp.GetElementsByClassName("piano-modal")[0]).Remove()

        for element in document_amp.GetElementsByClassName("mobile-navigation") do
            element.Remove()

        let model   = { Text = document_amp.DocumentElement.OuterHtml }
        let view    = Views.article model
        htmlView view
    with
    | :? System.Net.WebException as ex ->
        displayError $"Something went wrong when loading the article: {ex.InnerException.Message}"
    | Failure ex ->
        displayError $"Caught an exception: {ex}"

let getArticleIlTirreno path =
    try
        let response = Http.Request(path, responseEncodingOverride = "utf-8")

        let html = ExtractValueFromBody response.Body
    
        let document = getHTMLDoc html

        for element in document.GetElementsByTagName("script") do
            element.Remove()

        let model   = { Text = document.DocumentElement.OuterHtml }
        let view    = Views.article model
        htmlView view
    with
    | :? System.Net.WebException as ex ->
        displayError $"Something went wrong when loading the article: {ex.InnerException.Message}"
    | Failure ex ->
        displayError $"Caught an exception: {ex}"
