namespace Rhino.Scripting.FSharp

open System
open Rhino
open Rhino.Geometry
open Rhino.Scripting.RhinoScriptingUtils
open UtilRHinoScriptingFSharp
open Rhino.Scripting

/// This module provides curried functions to manipulate Rhino Point3d
/// It is NOT automatically opened.
[<RequireQualifiedAccess>]
module RhPoints =


    /// returns the closest point index from a Point list to a given Point
    let closestPointIdx (pt:Point3d) (pts:ResizeArray<Point3d>) : int =
        if pts.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.closestPoint empty List of Points: pts"
        let mutable mi = -1
        let mutable mid = Double.MaxValue
        for i=0 to pts.Count-1 do
            let p = pts.[i]
            let d = Point3d.distanceSq p pt
            if d < mid then
                mid <- d
                mi <- i
        mi

    /// returns the closest point from a Point list to a given Point
    let closestPoint (pt:Point3d) (pts:ResizeArray<Point3d>) : Point3d=
        pts.[closestPointIdx pt pts]

    /// returns the indices of the points that are closest to each other
    let closestPointsIdx (xs:ResizeArray<Point3d>) (ys:ResizeArray<Point3d>) =
        if xs.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.closestPointsIdx empty List of Points: xs"
        if ys.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.closestPointsIdx empty List of Points: ys"
        let mutable xi = -1
        let mutable yj = -1
        let mutable mid = Double.MaxValue
        for i=0 to xs.Count-1 do
            let pt = xs.[i]
            for j=0 to ys.Count-1 do
                let d = Point3d.distanceSq pt ys.[j]
                if d < mid then
                    mid <- d
                    xi <- i
                    yj <- j
        xi,yj

    /// returns the smallest Distance between Point Sets
    let minDistBetweenPointSets (xs:ResizeArray<Point3d>) (ys:ResizeArray<Point3d>) =
        if xs.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.minDistBetweenPointSets empty List of Points: xs"
        if ys.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.minDistBetweenPointSets empty List of Points: ys"
        let (i,j) = closestPointsIdx xs ys
        Point3d.distance xs.[i]  ys.[j]

    /// find the index of the point that has the biggest distance to any point from the other set
    /// basically the most lonely point in 'findPointFrom' list with respect to 'checkAgainst' list
    /// returns findPointFromIdx * checkAgainstIdx
    let mostDistantPointIdx (findPointFrom:ResizeArray<Point3d>) (checkAgainst:ResizeArray<Point3d>) : int*int=
        if findPointFrom.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.mostDistantPoint empty List of Points: findPointFrom"
        if checkAgainst.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.mostDistantPoint empty List of Points: checkAgainst"
        let mutable maxd = Double.MinValue
        let mutable findPointFromIdx = -1
        let mutable checkAgainstTempIdx = -1
        let mutable checkAgainstIdx = -1
        for i=0 to findPointFrom.Count-1 do
            let pt = findPointFrom.[i]
            let mutable mind = Double.MaxValue
            for j=0 to checkAgainst.Count-1 do
                let d = Point3d.distanceSq pt checkAgainst.[j]
                if d < mind then
                    mind <- d
                    checkAgainstTempIdx <-j
            if mind > maxd then
                maxd <- mind
                findPointFromIdx <-i
                checkAgainstIdx <-checkAgainstTempIdx
        findPointFromIdx, checkAgainstIdx

    /// find the point that has the biggest distance to any point from another set
    let mostDistantPoint (findPointFrom:ResizeArray<Point3d>) (checkAgainst:ResizeArray<Point3d>) =
        let i,_ = mostDistantPointIdx findPointFrom checkAgainst
        findPointFrom.[i]


    /// Culls points if they are too close to previous or next item
    /// Last and first points stay the same
    let cullDuplicatePointsInSeq (tolerance) (pts:ResizeArray<Point3d>)  =
        if pts.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.cullDuplicatePointsInSeq empty List of Points: pts"
        if pts.Count = 1 then
            pts
        else
            let tolSq = tolerance*tolerance
            let res  =  ResizeArray(pts.Count)
            let mutable last  = pts.[0]
            res.Add last
            let iLast = pts.Count-1
            for i = 1  to iLast do
                let pt = pts.[i]
                if Point3d.distanceSq last pt > tolSq then
                    last <- pt
                    res.Add last
                elif i=iLast then // to ensure last point stays the same
                    res.RemoveAt(res.Count-1)
                    res.Add pt
            res




    let internal minIndexBy (projection : 'T -> 'Key) (xs: ResizeArray<'T>) : int =
        if xs.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.ResizeArr.minIndexBy: Failed on empty." // noReflection for Fable. <%O>" typeof<'T>
        let mutable f = projection xs.[0]
        let mutable mf = f
        let mutable ii = 0
        for i=1 to xs.Count-1 do
            f <- projection xs.[i]
            if f < mf then
                ii <- i
                mf <- f
        ii
    let internal maxIndexBy (projection : 'T -> 'Key) (xs: ResizeArray<'T>) : int =
        if xs.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.ResizeArr.maxIndexBy: Failed on empty ResizeArray." // noReflection for Fable. <%O>" typeof<'T>
        let mutable f = projection xs.[0]
        let mutable mf = f
        let mutable ii = 0
        for i=1 to xs.Count-1 do
            f <- projection xs.[i]
            if f > mf then
                ii <- i
                mf <- f
        ii

    let internal last (xs: ResizeArray<'T>) : 'T =
        if xs.Count = 0 then RhinoScriptingFSharpException.Raise "RhPoints.ResizeArr |> last: Failed on empty ResizeArray." // noReflection for Fable. <%O>" typeof<'T>
        xs.[xs.Count-1]

    let internal rev (xs: ResizeArray<'T>) : ResizeArray<'T> =
        let res = ResizeArray(xs.Count)
        for i = xs.Count-1 downto 0 do
            res.Add xs.[i]
        res

    let pop i (xs: ResizeArray<'T>) : 'T =
        let res = xs.[i]
        xs.RemoveAt(i)
        res


    /// Similar to Join Polylines, this tries to find continuous sequences of points.
    /// 'tolGap' is the maximum allowable gap between the start and the endpoint of two segments.
    /// Search starts from the segment with the most points.
    /// Both start and end point of each point list is checked for adjacency
    let findContinuousPoints (tolGap:float)  (ptss: ResizeArray<ResizeArray<Point3d>>)  =
        let i =  ptss |> maxIndexBy (fun a -> a.Count)
        let res = ptss.[i]
        ptss.RemoveAt(i)
        let mutable loop = true
        while loop && ptss.Count > 0 do
            //first try to append to end
            let ende = res |> last
            let si = ptss |> minIndexBy ( fun ps -> Point3d.distanceSq ende ps.[0])
            let ei = ptss |> minIndexBy ( fun ps -> Point3d.distanceSq ende (ps |> last))
            let sd = Point3d.distance ende ptss.[si].[0]
            let ed = Point3d.distance ende (ptss.[ei] |> last)
            if   sd < tolGap && sd < ed then  res.AddRange(    ptss|> pop si)
            elif ed < tolGap && ed < sd then  res.AddRange(rev(ptss|> pop ei))
            else
                //search from start
                let start = res.[0]
                let si = ptss |> minIndexBy ( fun ps -> Point3d.distanceSq start ps.[0])
                let ei = ptss |> minIndexBy ( fun ps -> Point3d.distanceSq start (ps |> last))
                let sd = Point3d.distance start ptss.[si].[0]
                let ed = Point3d.distance start (ptss.[ei] |> last)
                if   sd < tolGap && sd < ed then res.InsertRange(0, rev(ptss|> pop si))
                elif ed < tolGap && ed < sd then res.InsertRange(0,     ptss|> pop ei)
                else
                    loop <- false
        res

