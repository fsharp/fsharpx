(*** hide ***)
// This block of code is omitted in the generated HTML documentation. Use 
// it to define helpers that you do not want to show in the documentation.
#I "../../bin/v4.0"

(** 
# FSharpx.Core and other Extras

## FSharpx.Core

FSharpx.Core currently implements:

* Several standard monads: State, Reader, Writer, Either, Continuation, Distribution

* Validation applicative functor

* General functions like flip

* Some asynchronous programming utilities

* Functions to make C# - F# interop easier.

FSharpx.Core is a set of additions on top of FSharpx.Collections


## Extras

These are some adhoc components which are now rarely used:

* FSharpx.Http provides a few features for working with HTTP applications.
* FSharpx.Observable implements a mini-Reactive Extensions (MiniRx) and was authored by Phil Trelford.
* FSharpx.Text.StructuredFormat provides data structures and functoins for pretty printers.


*)

#r "FSharpx.Collections.dll"
#r "FSharpx.Core.dll"

open FSharpx.Functional

open FSharpx.Collections

// Access the object that proves that 'List' is a 'monoid'

FSharpx.Collections.List.monoid

(**

## Examples

* [AsyncFileExtensions](AsyncFileExtensions.html)
* [AsyncSeqObservable](AsyncSeqObservable.html)
* [AutoCancel](AutoCancel.html)
* [BatchProcessing](BatchProcessing.html)
* [BlockingQueue](BlockingQueue.html)
* [Caching](Caching.html)
* [ChatServer](ChatServer.html)
* [CircularBuffer](CircularBuffer.html)
* [Crawler](Crawler.html)
* [DiningPhilosophers](DiningPhilosophers.html)
* [MouseFollow](MouseFollow.html)
* [Santa](Santa.html)
* [StmSample](StmSample.html)
* [StockStream](StockStream.html)
* [UndoSample](UndoSample.html)
* [WebProxy](WebProxy.html)

*)
