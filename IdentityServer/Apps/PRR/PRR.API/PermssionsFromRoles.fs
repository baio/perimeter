﻿namespace PRR.API

open System.Threading.Tasks

[<AutoOpen>]
module PermissionsFromRoles =
    
    type GetPermissions = int seq -> Task<int seq>

    type IPermissionsFromRoles =
        abstract GetPermissions: GetPermissions
