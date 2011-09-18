﻿module FSharpx.JSON

// Initial version of the parser from http://blog.efvincent.com/parsing-json-using-f/
// Simplyfied, added AST and fixed some minor bugs

open System

type Token =
| OpenBracket | CloseBracket
| OpenArray | CloseArray
| Colon | Comma
| String of string
| Boolean of bool
| Null
| Number of string

let tokenize source=
    let rec parseString acc = function
        | '\\' :: '"' :: t -> // escaped quote
            parseString (acc + "\"") t
        | '"' :: t -> // closing quote terminates    
            acc, t
        | c :: t -> // otherwise accumulate
            parseString (acc + (c.ToString())) t
        | _ -> failwith "Malformed string."
 
    let rec token acc = function
        | (')' :: _) as t -> acc, t // closing paren terminates
        | ('}' :: _) as t -> acc, t // closing paren terminates
        | (':' :: _) as t -> acc, t // colon terminates
        | (',' :: _) as t -> acc, t // comma terminates
        | w :: t when Char.IsWhiteSpace(w) -> acc, t // whitespace terminates
        | [] -> acc, [] // end of list terminates
        | c :: t -> token (acc + (c.ToString())) t // otherwise accumulate chars

    let rec tokenize' acc = function
        | w :: t when Char.IsWhiteSpace(w) -> tokenize' acc t   // skip whitespace
        | '{' :: t -> tokenize' (OpenBracket :: acc) t
        | '}' :: t -> tokenize' (CloseBracket :: acc) t
        | '[' :: t -> tokenize' (OpenArray :: acc) t
        | ']' :: t -> tokenize' (CloseArray :: acc) t
        | ':' :: t -> tokenize' (Colon :: acc) t
        | ',' :: t -> tokenize' (Comma :: acc) t
        | 'n' :: 'u' :: 'l' :: 'l' :: t -> tokenize' (Token.Null :: acc) t
        | 't' :: 'r' :: 'u' :: 'e' :: t -> tokenize' (Boolean true :: acc) t
        | 'f' :: 'a' :: 'l' :: 's' :: 'e' :: t -> tokenize' (Boolean false :: acc) t
        | '"' :: t -> // start of string
            let s, t' = parseString "" t
            tokenize' (Token.String(s) :: acc) t'        
        | '-' :: d :: t when Char.IsDigit(d) -> // start of negative number
            let n, t' = token ("-" + d.ToString()) t
            tokenize' (Token.Number(n) :: acc) t'
        | '+' :: d :: t 
        | d :: t when Char.IsDigit(d) -> // start of positive number
            let n, t' = token (d.ToString()) t
            tokenize' (Token.Number(n) :: acc) t'
        | [] -> List.rev acc // end of list terminates
        | _ -> failwith "Tokinzation error"

    tokenize' [] [for x in source -> x]

#if INTERACTIVE
#r "System.Xml.Linq.dll"
#endif
open System.Xml.Linq

type JSON =
| Text of string
| Number of float
| Boolean of bool
| Null
| JArray of JSON list
| JObject of Map<string,JSON>

    member json.ToXml() =
        let rec toXml' json =
            match json with
            | Text(str) -> str :> obj
            | Number(int) -> int :> obj
            | Boolean(bool) -> bool :> obj
            | Null -> null
            | JArray(array) -> array |> Seq.map (fun item -> new XElement(XName.Get "item", toXml' item)) :> obj
            | JObject(map) -> map |> Map.toSeq |> Seq.map (fun (k,v) -> new XElement(XName.Get k, toXml' v)) :> obj 
        toXml' json :?> XElement seq

    override json.ToString() =
        let strCat sep (seq:string seq) =
            seq |> Seq.fold (fun s1 s2 -> if s1 = "" then s2 else s1 + sep + s2) ""

        match json with
        | Text(str) -> "\"" + str + "\""
        | Number(int) -> string int
        | Boolean(bool) -> if bool then "true" else "false"
        | Null -> "null"
        | JArray(array) -> "[" + (array |> Seq.map (fun item -> item.ToString()) |> strCat ", ") + "]"
        | JObject(map) -> "{" + (map |> Map.toSeq |> Seq.map (fun (k,v) -> "\"" + k + "\": " + v.ToString()) |> strCat ", ") + "}"    

let emptyJObject = JObject Map.empty
let addProperty key value = function
| JObject(properties) -> JObject(Map.add key value properties)
| _ -> failwith "Malformed JSON object" 


/// Parses a JSON source text and returns an JSON AST
let parse source =
    let map = function
    | Token.Number number -> Number (Double.Parse number) 
    | Token.String text -> Text text
    | Token.Null -> JSON.Null
    | Token.Boolean(b) -> Boolean b
    | v -> failwith "Syntax Error, unrecognized token in map()"
 
    let rec parseValue = function
    | OpenBracket :: t -> parseJObject t
    | OpenArray :: t ->  parseArray t
    | h :: t -> map h, t
    | _ -> failwith "bad value"
 
    and parseArray = function
    | Comma :: t -> parseArray t
    | CloseArray :: t -> JArray [], t
    | t ->        
        let element, t' = parseValue t
        match parseArray t' with
        | JArray(elements),t'' -> JArray (element :: elements),t''
        | _ -> failwith "Malformed JSON array"
    and parseJObject = function
    | Comma :: t -> parseJObject t
    | Token.String(name) :: Colon :: t -> 
        let value,t' = parseValue t
        let jObject,t'' = parseJObject t'
        addProperty name value jObject,t''
    | CloseBracket :: t -> emptyJObject, t
    | _ -> failwith "Malformed JSON object"
    
    tokenize source 
    |> parseValue
    |> fst
