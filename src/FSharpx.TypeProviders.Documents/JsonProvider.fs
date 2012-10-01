﻿module internal FSharpx.TypeProviders.JsonTypeProvider

open System
open System.IO
open FSharpx.TypeProviders.DSL
open Microsoft.FSharp.Quotations
open Samples.FSharp.ProvidedTypes
open FSharpx.TypeProviders.Inference
open FSharpx.JSON.DocumentExtensions
open FSharpx.JSON

// Generates type for an inferred JSON document
let rec generateType (ownerType:ProvidedTypeDefinition) (CompoundProperty(elementName,multiProperty,elementChildren,elementProperties)) =
    let ty = runtimeType<IDocument> elementName
    ownerType.AddMember(ty)

    let accessExpr propertyName propertyType (args: Expr list) = 
        match propertyType with
        | x when x = typeof<string> -> 
            <@@ (%%args.[0]: IDocument).GetText propertyName @@>
        | x when x = typeof<bool> -> 
            <@@ (%%args.[0]: IDocument).GetBoolean propertyName @@>
        | x when x = typeof<int> -> 
            <@@ (%%args.[0]: IDocument).GetNumber propertyName |> int @@>
        | x when x = typeof<float> -> 
            <@@ (%%args.[0]: IDocument).GetNumber propertyName @@>
        | x when x = typeof<DateTime> -> 
            <@@ (%%args.[0]: IDocument).GetDate propertyName @@>

    let checkIfOptional propertyName (args: Expr list) = 
        <@@ (%%args.[0]: IDocument).HasProperty propertyName @@>

    let setterExpr propertyName propertyType (args: Expr list) = 
        match propertyType with
        | x when x = typeof<string> -> 
            <@@ (%%args.[0]: IDocument).AddTextProperty(propertyName,(%%args.[1]:string)) |> ignore @@>
        | x when x = typeof<bool> -> 
            <@@ (%%args.[0]: IDocument).AddBoolProperty(propertyName,(%%args.[1]:bool)) |> ignore @@>
        | x when x = typeof<int> ->
            <@@ (%%args.[0]: IDocument).AddNumberProperty(propertyName,float (%%args.[1]:int)) |> ignore @@>
        | x when x = typeof<float> ->
            <@@ (%%args.[0]: IDocument).AddNumberProperty(propertyName,(%%args.[1]:float)) |> ignore @@>
        | x when x = typeof<DateTime> -> 
            <@@ (%%args.[0]: IDocument).AddDateProperty(propertyName,(%%args.[1]:DateTime)) |> ignore @@>

    let optionalSetterExpr propertyName propertyType (args: Expr list) =         
        match propertyType with
        | x when x = typeof<string> -> 
            <@@ match (%%args.[1]:string option) with
                | Some text -> (%%args.[0]: IDocument).AddTextProperty(propertyName,text) |> ignore
                | None -> (%%args.[0]: IDocument).RemoveProperty propertyName @@>
        | x when x = typeof<bool> -> 
            <@@ match (%%args.[1]:bool option) with
                | Some boolean -> (%%args.[0]: IDocument).AddBoolProperty(propertyName,boolean) |> ignore
                | None -> (%%args.[0]: IDocument).RemoveProperty propertyName @@>
        | x when x = typeof<int> -> 
            <@@ match (%%args.[1]:int option) with
                | Some number -> (%%args.[0]: IDocument).AddNumberProperty(propertyName,float number) |> ignore
                | None -> (%%args.[0]: IDocument).RemoveProperty propertyName @@>
        | x when x = typeof<float> -> 
            <@@ match (%%args.[1]:float option) with
                | Some number -> (%%args.[0]: IDocument).AddNumberProperty(propertyName,number) |> ignore
                | None -> (%%args.[0]: IDocument).RemoveProperty propertyName @@>
        | x when x = typeof<DateTime> -> 
            <@@ match (%%args.[1]:DateTime option) with
                | Some date -> (%%args.[0]: IDocument).AddDateProperty(propertyName,date) |> ignore
                | None -> (%%args.[0]: IDocument).RemoveProperty propertyName @@>

    generateProperties ty accessExpr checkIfOptional setterExpr optionalSetterExpr elementProperties
    
    let multiAccessExpr childName (args: Expr list) = <@@ (%%args.[0]: IDocument).GetJArray(childName).Elements @@>
    let singleAccessExpr childName (args: Expr list) = <@@ (%%args.[0]: IDocument).GetProperty childName @@>
    let newChildExpr childName (args: Expr list) = <@@ JObject.New() @@>

    let addChildExpr childName (args: Expr list) = <@@ (%%args.[0]: IDocument).GetJArray(childName).Elements.Add(%%args.[1]:IDocument) @@>

    generateSublements ty ownerType multiAccessExpr addChildExpr newChildExpr singleAccessExpr generateType elementChildren

open System.Xml
open System.Xml.Linq

/// Infer schema from the loaded data and generate type with properties
let jsonType (ownerType:TypeProviderForNamespaces) cfg =     
    let createTypeFromSchema typeName (jsonText:string) =
        { Schema = JSONInference.provideElement "Document" false [parse jsonText]
          EmptyConstructor = fun args -> <@@ parse jsonText @@>
          FileNameConstructor = fun args -> <@@ (%%args.[0] : string) |> File.ReadAllText |> parse  @@>
          DocumentContentConstructor = fun args -> <@@ (%%args.[0] : string) |> parse  @@>
          RootPropertyGetter = fun args -> <@@ (%%args.[0] : IDocument) @@>
          ToStringExpr = fun args -> <@@ (%%args.[0]: IDocument).ToString() @@> }
        |> createParserType<IDocument> typeName generateType            
        |+!> (provideMethod ("ToXml") [] typeof<XObject seq> (fun args -> <@@ (%%args.[0]: IDocument).ToXml() @@>)
                |> addMethodXmlDoc "Gets the Xml representation")

    let createTypeFromFileName typeName = File.ReadAllText >> createTypeFromSchema typeName

    createStructuredParser thisAssembly rootNamespace "StructuredJSON" cfg ownerType createTypeFromFileName createTypeFromSchema