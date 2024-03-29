﻿namespace PRR.Domain.Auth

open PRR.Data.DataContext
open PRR.Data.Entities
open DataAvail.EntityFramework.Common

module Helpers =

    let private getOne x =
        CRUD.getOne<_, int, int, DbDataContext> x

    let getTenantIdFromUserId: CRUD.GetOne<int, int, DbDataContext> =
        getOne<Tenant> (<@ fun x id -> x.UserId = id @>) (<@ fun p -> p.Id @>)
