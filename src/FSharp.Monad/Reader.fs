﻿namespace FSharp.Monad
/// The reader monad.
/// This monad comes from Matthew Podwysocki's http://codebetter.com/blogs/matthew.podwysocki/archive/2010/01/07/much-ado-about-monads-reader-edition.aspx.
module Reader =
  open System

  type Reader<'r, 'a> = Reader of ('r -> 'a)
 
  let runReader (Reader r) env = r env
  type ReaderBuilder() =
    member this.Return(a) = Reader (fun _ -> a)
    member this.ReturnFrom(a:Reader<'r,'a>) = a
    member this.Bind(m, k) = Reader (fun r -> runReader (k (runReader m r)) r)
    member this.Zero() = this.Return ()
    member this.Combine(r1, r2) = this.Bind(r1, fun () -> r2)
    member this.TryWith(m, h) =
      Reader (fun env -> try runReader m env
                         with e -> runReader (h e) env)
    member this.TryFinally(m, compensation) =
      Reader (fun env -> try runReader m env
                         finally compensation())
    member this.Using(res:#IDisposable, body) =
      this.TryFinally(body res, (fun () -> match res with null -> () | disp -> disp.Dispose()))
    member this.Delay(f) = this.Bind(this.Return (), f)
    member this.While(guard, m) =
      if not(guard()) then this.Zero() else
        this.Bind(m, (fun () -> this.While(guard, m)))
    member this.For(sequence:seq<_>, body) =
      this.Using(sequence.GetEnumerator(),
                 (fun enum -> this.While(enum.MoveNext, this.Delay(fun () -> body enum.Current))))
  let reader = new ReaderBuilder()

  let ask = Reader(id)
  let asks f = reader {
    let! r = ask
    return (f r) }
  let local f m = Reader (f >> runReader m)

  module Operators =
    let inline mreturn x = reader.Return x
    let inline (>>=) m f = reader.Bind(m, f)
    let inline (<*>) f m = f >>= fun f' -> m >>= fun m' -> mreturn (f' m')
    let inline lift f m = mreturn f <*> m
    let inline (<!>) f m = lift f m
    let inline lift2 f a b = mreturn f <*> a <*> b
    let inline ( *>) x y = lift2 (fun _ z -> z) x y
    let inline ( <*) x y = lift2 (fun z _ -> z) x y
    let inline (>>.) m f = reader.Bind(m, fun _ -> f)