// ---------------------------------
// Handlers
// ---------------------------------

module Handlers

open Giraffe.Core
open Utils
open Bypass
open Views

let indexHandler () =
    let view      = Views.index ()
    htmlView view

let articleHandler (url: string) =
    let urlDecoded = DecodeUrl url

    let realDomain = getDomain urlDecoded

    match realDomain with
    | "huffingtonpost.it" -> getArticleWithGoogle urlDecoded
    | "repubblica.it" -> getArticleWithGoogle urlDecoded
    | "limesonline.com" -> getArticleWithGoogle urlDecoded
    | "quotidiano.net" -> getArticleLaNazione urlDecoded
    | "gelocal.it" -> getArticleIlTirreno urlDecoded
    | _ -> displayError "URL must be a supported website, my friend."

let errorPageHandler (err: string) =
    let model = { Text = err }
    let view = Views.errorPage model
    htmlView view

