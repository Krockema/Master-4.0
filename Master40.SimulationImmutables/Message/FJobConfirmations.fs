﻿module FJobConfirmations

open IJobs
open FSetupDefinitions

    type public FJobConfirmation = {
        Job : IJob
        Schedule : int64
        Duration : int64
        SetupDefinition : FSetupDefinition
    } with member this.UpdateJob job = { this with Job = job }
           member this.IsReset = this.Schedule.Equals(-1)