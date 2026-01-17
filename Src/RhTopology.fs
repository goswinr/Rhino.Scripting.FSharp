namespace Rhino.Scripting.FSharp

open Rhino
open Rhino.Geometry
open Rhino.Scripting
open Rhino.Scripting.RhinoScriptingUtils


[<RequireQualifiedAccess>]
module RhTopology =

    // The same function exists on FsEX.ResizeArray module too but with extra error checking.
    // Swap the values of two given indices in ResizeArray
    let inline private swap i j (xs:ResizeArray<'T>) :unit =
        if i<>j then
            let ti = xs.[i]
            xs.[i] <- xs.[j]
            xs.[j] <- ti

    // The same function exists on FsEX.ResizeArray module too but with extra error checking.
    // Like ResizeArray.minIndexBy but starting to search only from a given index
    let inline private minIndexByFrom  (compareBy: 'T -> 'U)  fromIdx (xs:ResizeArray<'T>) : int =
        let mutable idx = fromIdx
        let mutable mi = compareBy xs.[fromIdx]
        for j = fromIdx + 1 to xs.Count-1 do
            let this = compareBy xs.[j]
            if this < mi then
                idx <- j
                mi <- this
        idx

    /// Sorts elements in place to be in a circular structure.
    /// For each line endpoint, it finds the next closest line start point.
    /// (Does not check other line endpoints that might be closer.)
    /// Line is used as an abstraction to hold start and end of arbitrary object.
    let sortToLoop (getLine: 'T -> Line) (xs:ResizeArray<'T>)  =
        for i = 0 to xs.Count - 2 do // only run till second last
            let thisLine = getLine xs.[i]
            // TODO: could be optimized using an R-Tree for very large lists instead of minBy function
            let nextIdx = xs |> minIndexByFrom (fun c -> RhinoScriptSyntax.DistanceSquare ((getLine c).From ,  thisLine.To) ) (i+1)
            xs |> swap (i+1) nextIdx

    /// Sorts elements in place to be in a circular structure.
    /// For each line endpoint, it finds the next closest start point or endpoint.
    /// Line is used as an abstraction to hold start and end of arbitrary object.
    /// Reverses elements in place where required, using the reverseInPlace function
    /// that takes the index and the element as parameters.
    let sortToLoopWithReversing (getLine: 'T -> Line) (reverseInPlace: int -> 'T -> unit) (xs:ResizeArray<'T>) : unit =
        for i = 0 to xs.Count - 2 do // only run till second last
            let thisLine = getLine xs.[i]
            // TODO: could be optimized using an R-Tree for very large lists instead of minBy function
            let nextIdxSt = xs |> minIndexByFrom (fun c -> RhinoScriptSyntax.DistanceSquare ((getLine c).From ,  thisLine.To) ) (i+1)
            let nextIdxEn = xs |> minIndexByFrom (fun c -> RhinoScriptSyntax.DistanceSquare ((getLine c).To   ,  thisLine.To) ) (i+1)
            // check if closest endpoint is closer than closest start-point
            if  RhinoScriptSyntax.DistanceSquare ((getLine xs.[nextIdxSt]).From ,  thisLine.To) <=
                RhinoScriptSyntax.DistanceSquare ((getLine xs.[nextIdxEn]).To   ,  thisLine.To) then
                    xs |> swap (i+1) nextIdxSt
            else
                    reverseInPlace nextIdxEn xs.[nextIdxEn]
                    xs |> swap (i+1) nextIdxEn

