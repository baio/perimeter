namespace PRR.System.Models

open Akka.Actor
open Akkling

[<AutoOpen>]
module CQRSSystem =
    type ICQRSSystem =
        abstract System: ActorSystem
        abstract EventsRef: IActorRef<Events>
        abstract CommandsRef: IActorRef<Commands>
        abstract QueriesRef: IActorRef<Queries>
