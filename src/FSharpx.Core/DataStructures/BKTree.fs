﻿namespace FSharpx.DataStructures

// Ported from http://hackage.haskell.org/packages/archive/bktrees/latest/doc/html/src/Data-Set-BKTree.html

type 'a BKTree =
  | Node of 'a * int * Map<int,'a BKTree>
  | Empty

module BKTree =

    open FSharpx

    let isEmpty = function
        | Empty -> true
        | Node(_,_,_) -> false

    let empty = Empty

    let size = function
        | Node(_, s, _) -> s
        | Empty -> 0

    let singleton a = Node(a, 1, Map.empty)

    let rec private add distance a = function
        | Empty -> Node(a, 1 , Map.empty)
        | Node(b, size, dict) ->
            let recurse _ tree = add distance a tree
            let d = distance a b
            let dict = Map.insertWith recurse d (Node(a, 1, Map.empty)) dict
            Node(b, size + 1, dict)

    let rec private exists distance a = function
        | Empty -> false
        | Node(b, _, dict) ->
            let d = distance a b
            if d = 0 then true
            else
                match Map.tryFind d dict with
                | None -> false
                | Some tree -> exists distance a tree

    let private subTree d n dict =
        let (_, rightTree) = dict |> Map.splitWithKey ((>) (d-n-1))
        let (centerTree, _) = rightTree |> Map.splitWithKey ((>=) (d+n+1))
        centerTree

    let rec private existsDistance distance n a = function
        | Empty -> false
        | Node(b, _, map) ->
            let d = distance a b
            if d <= n then true
            else
                map
                |> subTree d n
                |> Map.valueList
                |> List.exists (existsDistance distance n a)

    let rec toList = function
        | Empty -> []
        | Node(a, _ , d) ->
            a :: (d |> Map.valueList |> List.collect toList)

    let toSeq tree = tree |> toList |> List.toSeq
    
    let toArray tree = tree |> toList |> List.toArray

    let rec private toListDistance distance n a = function
        | Empty -> []
        | Node(b, _, dict) ->
            let d = distance a b
            dict
            |> subTree d n
            |> Map.valueList
            |> List.collect (toListDistance distance n a)
            |> if d <= n then List.cons b else id

    let private ofCollection pred distance xs = xs |> pred (flip (add distance)) empty

    let private ofList distance xs = ofCollection List.fold distance xs

    let private ofSeq distance xs = ofCollection Seq.fold distance xs

    let private ofArray distance xs = ofCollection Array.fold distance xs

    let private concat distance xs = xs |> List.collect toList |> ofList distance

    let rec private delete distance a = function
        | Empty -> Empty
        | Node(b, _, map) ->
            let d = distance a b
            if d = 0 then concat distance (Map.valueList map)
            else
                let subtree = Map.updateWith (Some << (delete distance a)) d map
                let size = subtree |> Map.valueList |> List.map size |> List.sum |> (+) 1
                Node(b, size, subtree)

    type 'a Functions(distance: 'a -> 'a -> int) =
        member x.distance = distance
        member x.add a tree = add distance a tree
        member x.exists a tree = exists distance a tree
        member x.existsDistance n a tree = existsDistance distance n a tree
        member x.toListDistance n a tree = toListDistance distance n a tree
        member x.ofList xs = ofList distance xs
        member x.concat xs = concat distance xs
        member x.append t1 t2 = x.concat [t1; t2]
        member x.delete a tree = delete distance a tree
        member x.ofSeq xs = ofSeq distance xs
        member x.ofArray xs = ofArray distance xs

    let private hirschberg xs = function
        | [] -> List.length xs
        | ys ->
            let lys = List.length ys
            let startArr = [1..lys] |> List.map (fun x -> (x,x))
            xs
            |> Seq.zip (Seq.unfold (fun i -> Some(i+1, i+1)) 0)
            |> Seq.toList
            |> List.fold (fun arr (i,xi) ->
                ys
                |> List.zip (List.sortBy fst arr)
                |> List.mapAccum (fun (s,c) ((j,el),yj) ->
                    let nc = List.min [s + (if xi = yj then 0 else 1); el + 1; c + 1]
                    ((el,nc),(j,nc))
                ) (i - 1, i)
                |> snd
            ) startArr
            |> List.find (fst >> (=) lys)
            |> snd

    let private byteStringDistance  xs = function
        | ys when ByteString.isEmpty ys -> ByteString.length xs
        | ys ->
            let xs = ByteString.toList xs
            let ys = ByteString.toList ys
            hirschberg xs ys

    let Int = Functions(fun i j -> abs(i - j))
    let Char = Functions(fun (i: char) j -> abs((int i) - (int j)))

    [<GeneralizableValue>]
    let List<'a when 'a : equality> : 'a list Functions = Functions hirschberg

    let ByteString = Functions byteStringDistance
