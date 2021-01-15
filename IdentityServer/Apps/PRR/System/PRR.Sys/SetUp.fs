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

    type Config =
        { JournalConnectionString: string
          SnapshotConnectionString: string }

    let readConfig confFileName =

        let confPath =
            sprintf "%s/%s" (System.IO.Directory.GetCurrentDirectory()) confFileName

        System.IO.File.ReadAllText confPath

    type SystemActors =
        { System: IActorRefFactory
          Social: IActorRef<Social.Message> }

    let setUp config confFileName =

        let configFileContent = confFileName |> readConfig

        let configContent =
            [ sprintf "akka.persistence.journal.mongodb.connection-string = \"%s\"" config.JournalConnectionString
              sprintf
                  "akka.persistence.snapshot-store.mongodb.connection-string = \"%s\""
                  config.SnapshotConnectionString ]
            |> String.concat "\n"


        let configStr =
            [ configFileContent
              // configs from parameter wins
              configContent ]
            |> String.concat "\n"

        let parsedConfig =
            ConfigurationFactory.ParseString configStr

        let sys = System.create "prr-sys-new" parsedConfig

        let socialActor =
            spawn sys "social" (Social.Reducer.createReducer ())

        { System = sys; Social = socialActor }
