namespace PRR.API.Tests.Utils

open DataAvail.KeyValueStorage.Core
open Microsoft.Extensions.DependencyInjection
open PRR.API.Infra
open PRR.System.Models
open System.Threading
open FSharp.Control.Tasks.V2.ContextInsensitive

[<AutoOpen>]
module UserTestContext =

    type UserTestContext =
        { TestFixture: TestFixture
          ConfirmTokenWaitHandle: AutoResetEvent
          TenantWaitHandle: AutoResetEvent
          GetConfirmToken: unit -> string
          GetTenant: unit -> CreatedTenantInfo
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

        let systemEventHandled =
            function
            | UserSignedUpEvent data ->
                confirmToken <- data.Token
                confirmTokenWaitHandle.Set() |> ignore
            | UserTenantCreatedEvent data ->
                tenant <- Some data
                tenantWaitHandle.Set() |> ignore
            | CommandFailureEvent _ ->
                confirmTokenWaitHandle.Set() |> ignore
                tenantWaitHandle.Set() |> ignore
            | QueryFailureEvent _ ->
                confirmTokenWaitHandle.Set() |> ignore
                tenantWaitHandle.Set() |> ignore
            | ResetPasswordEvent (ResetPassword.TokenAdded (item)) ->
                resetPasswordToken <- Some item.Token
                resetPasswordTokenHandle.Set() |> ignore
            | _ -> ()

        let overrideKeyValueStorage (kvStorage: IKeyValueStorage) (services: IServiceCollection) =
            let kvStorage' =
                { new IKeyValueStorage with
                    member __.AddValue k v x =
                        task {
                            let! res = kvStorage.AddValue k v x
                            resetPasswordToken <- Some k
                            resetPasswordTokenHandle.Set() |> ignore
                            return res
                        }

                    member __.GetValue<'a> k x = kvStorage.GetValue<'a> k x

                    member __.RemoveValue<'a> k x = kvStorage.RemoveValue<'a> k x

                    member __.RemoveValuesByTag<'a> k x = kvStorage.RemoveValuesByTag<'a> k x }

            services.AddSingleton<IKeyValueStorage>(kvStorage')


        testFixture.OverrideServices(fun services ->

            let sp = services.BuildServiceProvider()
            let systemEnv = sp.GetService<SystemEnv>()
            let systemConfig = sp.GetService<SystemConfig>()

            let systemEnv =
                { systemEnv with
                      EventHandledCallback = systemEventHandled }

            let sys =
                PRR.System.Setup.setUp systemEnv systemConfig "akka.hocon"

            services.AddSingleton<ICQRSSystem>(fun _ -> sys)
            |> ignore
            //
            let sysConfig: PRR.Sys.SetUp.Config =
                { JournalConnectionString = systemConfig.JournalConnectionString
                  SnapshotConnectionString = systemConfig.SnapshotConnectionString }

            let sys1 =
                PRR.Sys.SetUp.setUp sysConfig "akka.hocon"

            services.AddSingleton<ISystemActorsProvider>(SystemActorsProvider sys1)
            |> ignore

            overrideKeyValueStorage (sp.GetService<IKeyValueStorage>()) services

            servicesOverridesFun services)


        { TestFixture = testFixture
          ConfirmTokenWaitHandle = confirmTokenWaitHandle
          TenantWaitHandle = tenantWaitHandle
          GetConfirmToken = fun () -> confirmToken
          GetTenant = fun () -> tenant.Value
          GetResetPasswordToken = fun () -> resetPasswordToken.Value
          ResetPasswordTokenHandle = resetPasswordTokenHandle }


    let createUserTestContext x =
        x
        |> createUserTestContextWithServicesOverrides (fun _ -> ())
