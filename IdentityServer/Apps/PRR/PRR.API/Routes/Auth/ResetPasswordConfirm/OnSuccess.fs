namespace PRR.API.Routes.Auth.ResetPasswordConfirm

open Akkling
open System.Threading.Tasks
open DataAvail.KeyValueStorage
open PRR.System.Models
open PRR.Domain.Auth.ResetPasswordConfirm

[<AutoOpen>]
module private OnSuccess =

    let onSuccess (KeyValueStorage: IKeyValueStorage): OnSuccess =
        fun email -> KeyValueStorage.RemoveValuesByTag<ResetPassword.Item>(email)
