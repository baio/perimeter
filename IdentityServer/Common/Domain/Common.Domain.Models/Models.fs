namespace Common.Domain.Models

open System.Threading.Tasks

[<AutoOpen>]
module Models =     

    type TenantId = int
    type DomainPoolId = int
    
    type DomainId = int
    
    type ClientId = string
    
    type RoleId = int
    
    type Scope = string
    
    type Uri = string
    
    type UserId = int

    type CompanyId = int

    type Token = string
    
    type Email = string

    type StringSalter = string -> string

    type HashProvider = unit -> string

    [<Measure>] type days

    [<Measure>] type hours

    [<Measure>] type minutes

    [<Measure>] type seconds
    
    [<Measure>] type milliseconds
       