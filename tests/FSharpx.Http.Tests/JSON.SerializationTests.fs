﻿module FSharp.Http.Tests.JSONWriterTests

open NUnit.Framework
open FSharpx.Http.JSON
open FsUnit

[<Test>] 
let ``Can serialize empty document``() = 
    emptyJObject.ToString()
    |> should equal "{}"

[<Test>] 
let ``Can serialize document with single property``() =
    (emptyJObject |> addProperty "firstName" (Text "John")).ToString()
    |> should equal "{\"firstName\":\"John\"}"


[<Test>] 
let ``Can serialize document with booleans``() =
    (emptyJObject |> addProperty "aa" (Boolean true) |> addProperty "bb" (Boolean false)).ToString()
    |> should equal "{\"aa\":true,\"bb\":false}"

[<Test>]
let ``Can serialize document with array, null and number``() =
    let text = "{\"items\":[{\"id\":\"Open\"},null,{\"id\":25}]}"
    let json = parse text
    json.ToString() |> should equal text

open System.Xml.Linq

[<Test>]
let ``Can serialize document to XML``() =
    let text = "{\"items\": [{\"id\": \"Open\"}, null, {\"id\": 25}]}"
    let json = parse text
    let xml = json.ToXml() |> Seq.head 
    let expectedXml = XElement.Parse("<items><item><id>Open</id></item><item /><item><id>25</id></item></items>")
    xml.ToString() |> should equal (expectedXml.ToString())

[<Test>] 
let ``Can serialize null``() = 
    toJSON null
    |> should equal JSON.Null

[<Test>] 
let ``Can serialize a simple string``() = 
    toJSON "simple text"
    |> should equal (JSON.Text "simple text")

[<Test>] 
let ``Can serialize a simple integer``() = 
    toJSON 23
    |> should equal (JSON.Number 23.)
    
[<Test>] 
let ``Can serialize a simple float``() = 
    toJSON 23.23
    |> should equal (JSON.Number 23.23)

[<Test>] 
let ``Can serialize a simple bool``() = 
    toJSON true
    |> should equal (JSON.Boolean true)
