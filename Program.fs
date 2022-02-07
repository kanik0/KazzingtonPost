module KazzingtonPost.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open FSharp.Data

// ---------------------------------
// Models
// ---------------------------------

type Message =
    {
        Text : string
    }

// ---------------------------------
// Views
// ---------------------------------

module Views =
    open Giraffe.ViewEngine

    let layout (content: XmlNode list) =
        html [] [
            head [] [
                meta [ _charset "utf-8" ]
                meta [ _name "viewport"; _content "width=device-width, initial-scale=1.0" ]
                title []  [ encodedText "KazzingtonPost" ]
                link [ _rel  "stylesheet"
                       _type "text/css"
                       _href "/main.css" ]
            ]
            body [] content
        ]
    
    let articleLayout (content: XmlNode list) =
        html [] [
            head [] [
                title [] [ encodedText "KazzingtonPost" ]    
            ]
            body [] content
        ]

    let partial () =
        div [ _class "jumbotron" ] [
            div [ _class "container" ] [
                br []
                h1 [ _class "post-title-main"; ] [ 
                    encodedText "KazzingtonPost" 
                ]
            ]
        ]


    let index () =
        [
            partial()
            let KP_VERSION = "0.5.0"

            div [ _class "post-content container"; _style "text-align: center;"] [
                br []
                label [_for "url" ] [ rawText "Hello, gimme article URL PLZ:" ]
                br []
                br []
                input [ _id "url"; _type "text"; _name "url" ]
                br []
                input [ _class "button"; _id "btn"; _type "button"; _value "Get the stuff"; _onclick "btnClick()" ]

                br []
                br []
                p [] [ rawText $"<em>v.{KP_VERSION} - Written in F#</em>" ]
            ]

            script [ _type "application/javascript"] [
                rawText """
                function btnClick() { location.href = '/article/' + btoa(document.getElementById('url').value).replaceAll('+', '-').replaceAll('/', '_') }

                var input = document.getElementById("url");
                input.addEventListener("keyup", function(event) {
                  if (event.keyCode === 13) {
                    event.preventDefault();
                    document.getElementById("btn").click();
                  }
                });
                """
            ]
            
        ] |> layout

    let article (model : Message) =
        [
            div [] [
                p [] [ rawText model.Text]
            ]
        ] |> articleLayout

    let errorPage (model : Message) =
        [
            br []
            div [ _style "text-align: center;"] [
                p [] [ rawText model.Text]
            ]
        ] |> layout

// ---------------------------------
// Utils
// ---------------------------------

let ExtractValueFromBody (body: HttpResponseBody) =
    match body with
    | Text x -> x
    | Binary x -> "Nothing to do."

let (|Prefix|_|) (p:string) (s:string) =
    match s.StartsWith(p) with
    | true -> Some(s.Substring(p.Length))
    | _ -> None

let displayError errorMessage =
    let model = { Text = errorMessage }
    let view = Views.errorPage model
    htmlView view

// ---------------------------------
// Web app
// ---------------------------------

let indexHandler () =
    let view      = Views.index ()
    htmlView view

let articleHandler (url: string) =
    let urlDecoded = 
        url.Replace("-", "+").Replace("_", "/")
        |> System.Convert.FromBase64String
        |> System.Text.Encoding.ASCII.GetString

    let getArticle path =
        try
            let response = Http.Request(path,
                headers = [
                    "User-Agent", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                    "X-Forwarded-For", "66.249.66.1";
                    "Referer", "google.com"
                ]
            )
            let model   = { Text = ExtractValueFromBody response.Body }
            let view    = Views.article model
            htmlView view
        with
        | :? System.Net.WebException as ex ->
            displayError $"Something went wrong when loading the article: {ex.InnerException.Message}"
        | Failure ex ->
            displayError $"Caught an exception: {ex}"

    
    match urlDecoded with
    | Prefix "https://www.huffingtonpost.it/" rest | Prefix "https://huffingtonpost.it/" rest -> getArticle urlDecoded
    | Prefix "https://repubblica.it/" rest | Prefix "https://www.repubblica.it/" rest -> getArticle urlDecoded
    | Prefix "https://limesonline.com/" rest | Prefix "https://www.limesonline.com/" rest -> getArticle urlDecoded
    | _ -> displayError "URL must be a supported website, my friend."

let errorPageHandler (err: string) =
    let model = { Text = err }
    let view = Views.errorPage model
    htmlView view


let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> indexHandler ()
                routef "/article/%s" articleHandler
            ]
        setStatusCode 404 >=> errorPageHandler "404 - Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5000",
            "https://localhost:5001")
       .AllowAnyMethod()
       .AllowAnyHeader()
       |> ignore

let configureApp (app : IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()
    (match env.IsDevelopment() with
    | true  ->
        app.UseDeveloperExceptionPage()
    | false ->
        app .UseGiraffeErrorHandler(errorHandler)
            .UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    services.AddCors()    |> ignore
    services.AddGiraffe() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    builder.AddConsole()
           .AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "WebRoot")
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .UseContentRoot(contentRoot)
                    .UseWebRoot(webRoot)
                    .Configure(Action<IApplicationBuilder> configureApp)
                    .ConfigureServices(configureServices)
                    .ConfigureLogging(configureLogging)
                    |> ignore)
        .Build()
        .Run()
    0