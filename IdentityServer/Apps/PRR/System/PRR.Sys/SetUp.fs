// TODO : Rename sys auth
namespace PRR.Sys

open System.Runtime.InteropServices.WindowsRuntime
open Akka.Actor
open Akka.Configuration
open Akkling

open PRR.Sys
open PRR.Sys.Models

[<AutoOpen>]
module SetUp =

    let readConfig confFileName =

        let confPath =
            sprintf "%s/%s" (System.IO.Directory.GetCurrentDirectory()) confFileName

        let conf = System.IO.File.ReadAllText confPath

        ConfigurationFactory.ParseString conf

    type SystemActors =
        { System: IActorRefFactory
          Social: IActorRef<Social.Message> }

    let setUp confFileName =

        let sys =
            confFileName
            |> readConfig
            |> System.create "prr-sys-new"

        let socialActor =
            spawn sys "social" (Social.Reducer.createReducer ())

        { System = sys; Social = socialActor }
