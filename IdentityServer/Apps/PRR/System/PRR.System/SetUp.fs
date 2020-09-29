namespace PRR.System

open Akka.Actor
open Akka.Configuration
open Akkling
open Microsoft.Extensions.DependencyInjection
open PRR.Data.DataContext
open PRR.System.Models
open PRR.System.Views

[<AutoOpen>]
module Setup =

    type CQRSSystem =
        { System: ActorSystem
          EventsRef: IActorRef<Events>
          CommandsRef: IActorRef<Commands>
          QueriesRef: IActorRef<Queries> }
        interface ICQRSSystem with
            member __.System = __.System
            member __.CommandsRef = __.CommandsRef
            member __.EventsRef = __.EventsRef
            member __.QueriesRef = __.QueriesRef

    type DataContextProvider(serviceScope: IServiceScope) =
        let dataContext =
            serviceScope.ServiceProvider.GetService<DbDataContext>()

        interface IDataContextProvider with
            member __.DataContext = dataContext
            member __.Dispose() = serviceScope.Dispose()

    let setUp' env confFileName =
        let ss =
            Strategy.OneForOne(fun error ->
                printf "++++Error %O" error
                Directive.Escalate)

        let confPath =
            sprintf "%s/%s" (System.IO.Directory.GetCurrentDirectory()) confFileName

        let conf = System.IO.File.ReadAllText confPath

        let config = ConfigurationFactory.ParseString conf

        let sys = System.create "perimeter-sys" <| config // Configuration.defaultConfig()

        let rec events =
            spawn
                sys
                "events"
                { props (eventsHandler env (lazy (commands))) with
                      SupervisionStrategy = Some ss }

        and sharedActors =
            createSharedActors sys env (lazy (events)) ss

        and commands =
            spawn
                sys
                "commands"
                { props (commandsHandler env (lazy (events)) sharedActors) with
                      SupervisionStrategy = Some ss }

        let queries =
            spawn
                sys
                "queries"
                { props (queriesHandler sharedActors) with
                      SupervisionStrategy = Some ss }

        setUpViews sys sharedActors.LogInActor env.ViewsDbConnectionString

        { System = sys
          EventsRef = events
          CommandsRef = commands
          QueriesRef = queries } :> ICQRSSystem

    let setUp env = setUp' env "akka.hocon"
