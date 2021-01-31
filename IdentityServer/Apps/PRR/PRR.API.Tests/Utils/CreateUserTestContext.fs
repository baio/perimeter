namespace PRR.API.Tests.Utils

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra
open System.Threading
open FSharp.Control.Tasks.V2.ContextInsensitive
open PRR.Domain.Tenant
open DataAvail.KeyValueStorage.Core

[<AutoOpen>]
module UserTestContext =

    type UserTestContext =
        { TestFixture: TestFixture
          ConfirmTokenWaitHandle: AutoResetEvent
          TenantWaitHandle: AutoResetEvent
          GetConfirmToken: unit -> string
          GetTenant: unit -> CreatedTenantInfo
          SetTenant: CreatedTenantInfo -> unit
          GetResetPasswordToken: unit -> string
          ResetPasswordTokenHandle: AutoResetEvent }

    let createUserTestContextWithServicesOverrides servicesOverridesFun (testFixture: TestFixture) =


        let mutable confirmToken: string = null

        let confirmTokenWaitHandle =
            new System.Threading.AutoResetEvent(false)

        let mutable tenant: CreatedTenantInfo option = None

        let tenantWaitHandle =
            new System.Threading.AutoResetEvent(false)

        let mutable resetPasswordToken: string option = None

        let resetPasswordTokenHandle =
            new System.Threading.AutoResetEvent(false)

        let overrideKeyValueStorage (kvStorage: IKeyValueStorage) (services: IServiceCollection) =
            let kvStorage' =
                { new IKeyValueStorage with
                    member __.AddValue k (v: 'a) x =
                        task {
                            let! res = kvStorage.AddValue k v x

                            match getAddValuePartitionName<'a> x with
                            | "ResetPassword" ->
                                resetPasswordToken <- Some k
                                resetPasswordTokenHandle.Set() |> ignore
                            | "SignUp" ->
                                confirmToken <- k
                                confirmTokenWaitHandle.Set() |> ignore
                            | _ -> ()


                            return res
                        }

                    member __.GetValue<'a> k x = kvStorage.GetValue<'a> k x

                    member __.RemoveValue<'a> k x = kvStorage.RemoveValue<'a> k x

                    member __.RemoveValuesByTag<'a> k x = kvStorage.RemoveValuesByTag<'a> k x }

            services.AddSingleton<IKeyValueStorage>(kvStorage')


        testFixture.Server1.OverrideServices(fun services ->

            let sp = services.BuildServiceProvider()

            overrideKeyValueStorage (sp.GetService<IKeyValueStorage>()) services
            |> ignore

            servicesOverridesFun services)


        { TestFixture = testFixture
          ConfirmTokenWaitHandle = confirmTokenWaitHandle
          TenantWaitHandle = tenantWaitHandle
          GetConfirmToken = fun () -> confirmToken
          GetTenant = fun () -> tenant.Value
          GetResetPasswordToken = fun () -> resetPasswordToken.Value
          ResetPasswordTokenHandle = resetPasswordTokenHandle
          SetTenant = fun t -> tenant <- (Some t) }


    let createUserTestContext x =
        x
        |> createUserTestContextWithServicesOverrides (fun _ -> ())
