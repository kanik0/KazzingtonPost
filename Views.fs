// ---------------------------------
// Views
// ---------------------------------

module Views 

open Giraffe.ViewEngine

type Message =
    {
        Text : string
    }

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
        let KP_VERSION = "0.7.2"

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

