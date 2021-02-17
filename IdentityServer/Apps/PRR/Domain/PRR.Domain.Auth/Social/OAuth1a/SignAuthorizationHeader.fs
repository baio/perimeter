﻿namespace PRR.Domain.Auth.Social.OAuth1a

open System.Net
open System.Security.Cryptography
open System.Text

[<AutoOpen>]
module internal SignAuthorizationHeader =

    open System

    let signAuthorizationHeader
                                (methodName: string)
                                (requestUrl: string)
                                (consumerSecret: string)
                                (parameters: (string * string) seq)
                                (separator: string)
                                (tokenSecret: string option)
                                =
        let timestamp =
            (DateTime.UtcNow.Subtract(DateTime(1970, 1, 1)))
                .TotalSeconds.ToString()

        let nonce =
            Convert.ToBase64String(Encoding.ASCII.GetBytes(timestamp.ToString()))

        let signatureParameters =
            ([ ("oauth_nonce", nonce)
               ("oauth_signature_method", "HMAC-SHA1")
               ("oauth_timestamp", timestamp)
               ("oauth_version", "1.0") ])
            |> Seq.append parameters


        let signatureParameterString =
            signatureParameters
            |> Seq.map (fun (k, v) -> sprintf "%s=%s" k (WebUtility.UrlEncode v))
            |> String.concat "&"

        let signatureBaseString =
            methodName
            + "&"
            + WebUtility.UrlEncode(requestUrl)
            + "&"
            + WebUtility.UrlEncode(signatureParameterString)
            
        printfn "333 %s" signatureBaseString            

        let signingKey =
            match tokenSecret with
            | None -> WebUtility.UrlEncode(consumerSecret) + "&"
            | Some tokenSecret ->
                WebUtility.UrlEncode(consumerSecret)
                + "&"
                + WebUtility.UrlEncode(tokenSecret)
                
        printfn "111 %s" signingKey                

        let signatureBaseStringBytes =
            Encoding.ASCII.GetBytes(signatureBaseString)

        let signingKeyBytes = Encoding.ASCII.GetBytes(signingKey)

        let hmacSha1 = new HMACSHA1(signingKeyBytes)

        let signature =
            hmacSha1.ComputeHash signatureBaseStringBytes

        let base64Signature = Convert.ToBase64String signature

        let headerParameters =
            [ "oauth_signature", base64Signature ]
            |> Seq.append signatureParameters

        let headerParameterString =
            headerParameters
            |> Seq.map (fun (k, v) -> sprintf "%s=\"%s\"" k (WebUtility.UrlEncode v))
            |> String.concat separator

        printfn "222 %s" headerParameterString
        
        let authenticationHeaderValue = sprintf "OAuth %s" headerParameterString

        authenticationHeaderValue
