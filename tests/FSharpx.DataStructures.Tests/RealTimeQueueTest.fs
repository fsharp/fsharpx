﻿module FSharpx.DataStructures.Tests.RealTimeQueueTest

open System
open FSharpx.DataStructures
open FSharpx.DataStructures.RealTimeQueue
open NUnit.Framework
open FsUnit

[<Test>]
let ``empty queue should be empty``() =
    isEmpty empty |> should equal true

[<Test>]
let ``it should allow to enqueue``() =
    empty |> snoc 1 |> snoc 2 |> isEmpty |> should equal false

[<Test>]
let ``it should allow to dequeue``() =
    empty |> snoc 1 |> tail |> isEmpty |> should equal true

[<Test>]
let ``it should fail if there is no head in the queue``() =
    let ok = ref false
    try
        empty |> head |> ignore
    with x when x = Exceptions.Empty -> ok := true
    !ok |> should equal true

[<Test>]
let ``it should fail if there is no tail the queue``() =
    let ok = ref false
    try
        empty |> tail |> ignore
    with x when x = Exceptions.Empty -> ok := true
    !ok |> should equal true

[<Test>]
let ``it should allow to get the head from a non-queue``() =
    empty |> snoc 1 |> snoc 2 |> head |> should equal 1

[<Test>]
let ``it should allow to get the tail from the queue``() =
    empty |> snoc "a" |> snoc "b" |> snoc "c" |> tail |> head |> should equal "b"