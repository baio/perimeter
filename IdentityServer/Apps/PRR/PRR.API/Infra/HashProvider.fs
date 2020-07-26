namespace PRR.API.Infra

//https://stackoverflow.com/questions/38043954/generate-unique-hash-code-based-on-string

[<AutoOpen>]
module HashProvider =

    open System.Text
    open System.Security.Cryptography

    let getSha256Hash' (shaHash: SHA256) (input: string) =

        // Convert the input string to a byte array and compute the hash.
        let data = input |> Encoding.UTF8.GetBytes |> shaHash.ComputeHash

        // Create a new Stringbuilder to collect the bytes
        // and create a string.
        let sBuilder = StringBuilder()

        // Loop through each byte of the hashed data 
        // and format each one as a hexadecimal string.
        for i = 1 to data.Length - 1 do
            data.[i].ToString("x2") |> sBuilder.Append |> ignore
   
        // Return the hexadecimal string.
        sBuilder.ToString()

    let getSha256Hash (shaHash: SHA256) () =
        let input = System.Guid.NewGuid().ToString()
        getSha256Hash' shaHash input


    type HashProvider() = 
        let hash256 = SHA256.Create()        
        interface IHashProvider with
            member __.GetHash = (getSha256Hash hash256)
