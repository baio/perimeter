namespace PRR.API.Infra

open System

//https://stackoverflow.com/questions/38043954/generate-unique-hash-code-based-on-string

[<AutoOpen>]
module HashProvider =

    open System.Security.Cryptography
    open System.Text
    
    let getSha256Hash (shaHash: SHA256) (input: string) =
        input
        |> Encoding.UTF8.GetBytes
        |> shaHash.ComputeHash 
        

    let getSha256HexHash (shaHash: SHA256) (input: string) =
        // Convert the input string to a byte array and compute the hash.
        let data = getSha256Hash shaHash input
        
        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        let sBuilder = StringBuilder()

        // Loop through each byte of the hashed datar
        // and format each one as a hexadecimal string.
        for i = 1 to data.Length - 1 do
            data.[i].ToString("x2")
            |> sBuilder.Append
            |> ignore

        // Return the hexadecimal string.
        
        sBuilder.ToString() 

    let getGuidSha256Hash (shaHash: SHA256) () =
        let input = System.Guid.NewGuid().ToString()
        getSha256HexHash shaHash input

    type HashProvider(hash256: SHA256) =
        interface IHashProvider with
            member __.GetHash = (getGuidSha256Hash hash256)
