namespace Rhino.Scripting.FSharp

open System
open System.Collections.Generic
open Rhino
open Rhino.Geometry
open Rhino.Scripting
open Rhino.Scripting.RhinoScriptingUtils
open UtilRHinoScriptingFSharp
// open FsEx
// open FsEx.UtilMath
// open FsEx.SaveIgnore
// open FsEx.ExtensionsIList


/// This module provides functions to manipulate Rhino Vector3d
/// This module is automatically opened when Rhino.Scripting.FSharp namespace is opened.
/// These type extensions are only visible in F#.
[<AutoOpen>]
module AutoOpenVectors =

    type RhinoScriptSyntax with

        /// Draws a line with Curve Arrows from a given point.
        static member DrawVector(   vector:Vector3d,
                                    fromPoint:Point3d,
                                    [<OPT;DEF("")>]layer:string ) : Guid  =
            let l = RhinoScriptSyntax.AddLine(fromPoint, fromPoint + vector )
            RhinoScriptSyntax.CurveArrows(l, 2)
            if layer<>"" then RhinoScriptSyntax.ObjectLayer(l, layer, createLayerIfMissing=true)
            l

        /// Draws a line with Curve Arrows from World Origin.
        static member DrawVector( vector:Vector3d) : Guid  =
            let l = RhinoScriptSyntax.AddLine(Point3d.Origin, Point3d.Origin + vector )
            RhinoScriptSyntax.CurveArrows(l, 2)
            l

        ///<summary>Draws the axes of a Plane and adds TextDots to label them.</summary>
        ///<param name="pl">(Plane)</param>
        ///<param name="axLength">(float) Optional, Default Value: <c>1.0</c>, the size of the drawn lines</param>
        ///<param name="suffixInDot">(string) Optional, Default Value: no suffix, text to add to the TextDot labels of x, y, and z axes.</param>
        ///<param name="layer">(string) Optional, Default Value: the current layer, String for layer to draw plane on. The Layer will be created if it does not exist.</param>
        ///<returns>List of Guids of added Objects</returns>
        static member DrawPlane(    pl:Plane,
                                    [<OPT;DEF(1.0)>]axLength:float,
                                    [<OPT;DEF("")>]suffixInDot:string,
                                    [<OPT;DEF("")>]layer:string ) : ResizeArray<Guid>  =
            let a = RhinoScriptSyntax.AddLine(pl.Origin, pl.Origin + pl.XAxis*axLength)
            let b = RhinoScriptSyntax.AddLine(pl.Origin, pl.Origin + pl.YAxis*axLength)
            let c = RhinoScriptSyntax.AddLine(pl.Origin, pl.Origin + pl.ZAxis*axLength*0.5)
            let e = RhinoScriptSyntax.AddTextDot("x"+suffixInDot, pl.Origin + pl.XAxis*axLength)
            let f = RhinoScriptSyntax.AddTextDot("y"+suffixInDot, pl.Origin + pl.YAxis*axLength)
            let g = RhinoScriptSyntax.AddTextDot("z"+suffixInDot, pl.Origin + pl.ZAxis*axLength*0.5)
            let es = ResizeArray<Guid>(6)
            es.Add a
            es.Add b
            es.Add c
            es.Add e
            es.Add f
            es.Add g
            if layer <>"" then RhinoScriptSyntax.setLayers layer es
            let gg= RhinoScriptSyntax.AddGroup()
            RhinoScriptSyntax.AddObjectsToGroup(es, gg)
            es

        /// Returns a point that is at a given distance from a point in the direction of another point.
        /// Raises an exception if fromPt equals dirPt.
        static member DistPt(fromPt:Point3d, dirPt:Point3d, distance:float) : Point3d  =
            let v = dirPt - fromPt
            let len = v.Length
            if isTooTiny len then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.DistPt: fromPt %s and dirPt %s are too close" (pretty fromPt) (pretty dirPt)
            let sc = distance / len
            fromPt + v*sc

        /// Returns a Point by evaluating a line between two points with a normalized parameter.
        /// e.g. rel=0.5 will return the middle point, rel=1.0 the endPoint
        /// if the rel parameter is omitted it is set to 0.5
        static member DivPt(fromPt:Point3d, toPt:Point3d, [<OPT;DEF(0.5)>]rel:float) : Point3d  =
            let v = toPt - fromPt
            fromPt + v*rel


        /// Returns the average of many points.
        static member MeanPoint(pts:Point3d seq) : Point3d  =
            let mutable p = Point3d.Origin
            let mutable k = 0.0
            for pt in pts do
                k <- k + 1.0
                p <- p + pt
            p/k

        /// Finds the mean normal of many points.
        /// It finds the center point and then takes cross-products iterating all points in pairs of two.
        /// The first two points define the orientation of the normal.
        /// Considers current order of points too, counterclockwise in xy plane is z.
        static member NormalOfPoints(pts:Point3d IList) : Vector3d  =
            if pts.Count <= 2  then
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.NormalOfPoints can't find normal of two or fewer points %s" (pretty pts)
            elif pts.Count = 3 then
                let a = pts.[0] - pts.[1]
                let b = pts.[2] - pts.[1]
                let v= Vector3d.CrossProduct(b, a)
                if v.IsTiny() then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.NormalOfPoints: three points are in a line: %s" (pretty pts)
                else
                    v.Unitized
            else
                let cen = RhinoScriptSyntax.MeanPoint(pts)
                let mutable v = Vector3d.Zero
                // for t, n in Seq.thisNext pts do
                for i = 0 to pts.Count-1 do
                    let t = pts.[i]
                    let n = pts.[idxLooped (i+1) pts.Count]
                    let a = t-cen
                    let b = n-cen
                    let x = Vector3d.CrossProduct(a, b)  |> Vector3d.matchOrientation v // TODO do this matching?
                    v <- v + x
                if v.IsTiny() then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.NormalOfPoints: points are in a line: %s" (pretty pts)
                else
                    v.Unitized

        /// Calculates the intersection of a finite line with a triangle (without using Rhinocommon)
        /// Returns Some(Point3d) or None if no intersection found
        static member LineTriangleIntersect(line:Line, p1 :Point3d ,p2 :Point3d, p3 :Point3d) : Point3d option  =

            // https://stackoverflow.com/questions/42740765/intersection-between-line-and-triangle-in-3d
            /// computes the signed Volume of a Tetrahedron
            let inline tetrahedronVolumeSigned(a:Point3d, b:Point3d, c:Point3d, d:Point3d) =
                ((Vector3d.CrossProduct( b-a, c-a)) * (d-a)) / 6.0

            let q1 = line.From
            let q2 = line.To
            let s1 = sign (tetrahedronVolumeSigned(q1,p1,p2,p3))
            let s2 = sign (tetrahedronVolumeSigned(q2,p1,p2,p3))
            if s1 <> s2 then
                let s3 = sign (tetrahedronVolumeSigned(q1,q2,p1,p2))
                let s4 = sign (tetrahedronVolumeSigned(q1,q2,p2,p3))
                let s5 = sign (tetrahedronVolumeSigned(q1,q2,p3,p1))
                if s3 = s4 && s4 = s5 then
                    let n = Vector3d.CrossProduct(p2-p1,p3-p1)
                    let t = ((p1-q1) * n) / ((q2-q1) * n)
                    Some (q1 + t * (q2-q1))
                else None
            else None

        /// <summary>Offsets a Polyline in 3D space by finding the local offset in each corner.</summary>
        /// <param name="points"> List of points to offset. Auto detects if given points are from a closed Polyline (first point = last point) and loops them.</param>
        /// <param name="offsetDistances">Offset distances can vary per segment, Positive distance is offset inwards, negative outwards.
        ///     Distances Sequence  must have exact count , be a singleton ( for repeating) or empty seq ( for ignoring)</param>
        /// <param name="normalDistances">Normal distances define a perpendicular offset at each corner.
        ///     Distances Sequence  must have exact count , be a singleton ( for repeating) or empty seq ( for ignoring)</param>
        /// <param name="loop">Consider last point and first point to be from a closed loop, even if they are not at the same location.</param>
        /// <returns>A list of points that has the same length as the input list.</returns>
        static member OffsetPoints(     points: IList<Point3d>,  // IList so it can take a Point3dList class too
                                        offsetDistances: float seq,
                                        [<OPT;DEF(null:seq<float>)>] normalDistances: float seq,
                                        [<OPT;DEF(false)>]loop:bool) :Point3d  ResizeArray  =
            let offDists0  = Array.ofSeq offsetDistances
            let normDists0 = Array.ofSeq (normalDistances |? Seq.empty<float> )
            let pointCount = points.Count
            let lastIndex = pointCount - 1
            let lenDist = offDists0.Length
            let lenDistNorm = normDists0.Length
            if pointCount < 2 then
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints needs at least two points but %s given" (pretty points)
            elif pointCount = 2 then
                let offDist =
                    if   lenDist = 0 then 0.0
                    elif lenDist = 1 then offDists0.[0]
                    else RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints: offsetDistances has %d items but should have 1 or 0 for 2 given points %s" lenDist (pretty points)
                let normDist =
                    if   lenDistNorm = 0 then 0.0
                    elif lenDistNorm = 1 then normDists0.[0]
                    else RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints: normalDistances has %d items but should have 1 or 0 for 2 given points %s" lenDistNorm (pretty points)
                let a, b = Point3d.offsetTwoPt(points.[0], points.[1] , offDist, normDist)
                ResizeArray<Point3d> [|a; b|]
            else // regular case more than 2 points
                let lastIsFirst = (points.[0] - points.[points.Count-1]).Length < RhinoScriptSyntax.Doc.ModelAbsoluteTolerance //auto detect closed polyline points:
                let distsNeeded =
                    if lastIsFirst then pointCount - 1
                    elif loop      then pointCount
                    else                pointCount - 1
                let distsNeededNorm =
                    if lastIsFirst then pointCount - 1
                    elif loop      then pointCount
                    else                pointCount   // not -1 !!
                let  offDists =
                    if   lenDist = 0 then             Array.create distsNeeded 0.0
                    elif lenDist = 1 then             Array.create distsNeeded offDists0.[0]
                    elif lenDist = distsNeeded then   offDists0
                    else RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints: offsetDistances has %d items but should have %d (lastIsFirst=%b) (loop=%b)" lenDist distsNeeded lastIsFirst loop
                let normDists =
                    if   lenDistNorm = 0 then                 Array.create distsNeededNorm 0.0
                    elif lenDistNorm = 1 then                 Array.create distsNeededNorm normDists0.[0]
                    elif lenDistNorm = distsNeededNorm then   normDists0
                    else RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints: normalDistances has %d items but should have %d (lastIsFirst=%b) (loop=%b)" lenDistNorm distsNeededNorm lastIsFirst loop
                let refNormal = RhinoScriptSyntax.NormalOfPoints(points) //to have good starting direction, first kink might be in bad direction
                let Pts = ResizeArray<Point3d>(pointCount)
                let Ns = ResizeArray<Vector3d>(pointCount)
                // for i, p, t, n in Seq.iPrevThisNext(points) do
                for i = 0 to lastIndex do
                    let p = points[idxLooped (i-1) pointCount]
                    let t = points.[i]
                    let n = points[idxLooped (i+1) pointCount]
                    // first one:
                    if i=0 then
                        if lastIsFirst then
                            let prev = points[idxLooped -2 pointCount] //.GetNeg(-2) // because -1 is same as 0
                            let struct( _, _, pt, N) = Point3d.findOffsetCorner(prev, t, n, offDists.Last, offDists.[0], refNormal)
                            Pts.Add pt
                            Ns.Add N
                        else
                            let struct( _, sn, pt, N) = Point3d.findOffsetCorner(p, t, n, offDists.Last, offDists.[0], refNormal)
                            Ns.Add N
                            if loop then Pts.Add pt
                            else         Pts.Add (t + sn)
                    // last one:
                    elif i = lastIndex  then
                        if lastIsFirst then
                            let struct(_, _, pt, N) = Point3d.findOffsetCorner(p, t, points.[1], offDists.[i-1], offDists.[0], refNormal)
                            Pts.Add pt
                            Ns.Add N
                        elif loop then
                            let struct( _, _, pt, N) = Point3d.findOffsetCorner(p, t, n, offDists.[i-1], offDists.[i], refNormal)
                            Pts.Add pt
                            Ns.Add N
                        else
                            let struct( sp, _, _, N) = Point3d.findOffsetCorner(p, t, n, offDists.[i-1], offDists.[i-1], refNormal) // or any next off dist since only sp is used
                            Pts.Add (t + sp)
                            Ns.Add N
                    else
                        let struct( _, _, pt, N ) = Point3d.findOffsetCorner(p, t, n, offDists.[i-1], offDists.[i], refNormal)
                        Pts.Add pt
                        Ns.Add N
                if lenDistNorm > 0 then
                    for i=0 to  distsNeededNorm-1 do // ns might be shorter than pts if lastIsFirst= true
                        let n = Ns.[i]
                        if n <> Vector3d.Zero then
                            Pts.[i] <- Pts.[i] + n * normDists.[i]

                let rec searchBack i (ns:ResizeArray<Vector3d>) =
                    let ii = saveIdx (i) ns.Count
                    let v = ns.[ii]
                    if v <> Vector3d.Zero || i < -ns.Count then ii
                    else searchBack (i-1) ns

                let rec  searchForward i (ns:ResizeArray<Vector3d>) =
                    let ii = saveIdx (i) ns.Count
                    let v = ns.[ii]
                    if v <> Vector3d.Zero || i > (2 * ns.Count) then ii
                    else searchForward (i + 1) ns

                // fix collinear segments by nearest neighbors that are ok
                for i, n in Seq.indexed Ns do // ns might be shorter than pts if lastIsFirst= true
                    if n = Vector3d.Zero then
                        let pi = searchBack (i-1) Ns
                        let ppt = Pts.[pi]
                        let pln = Line(points.[pi], points.[saveIdx (pi + 1) pointCount])
                        let pclp = pln.ClosestPoint(ppt, limitToFiniteSegment=false)
                        let pv = ppt - pclp

                        let ni = searchForward (i + 1) Ns
                        let npt = Pts.[ni]
                        let nln = Line(points.[ni], points.[saveIdx (ni-1) pointCount])
                        let nclp = nln.ClosestPoint(npt, limitToFiniteSegment=false)
                        let nv = npt - nclp
                        //print (pi,"prev i")
                        //print (i,"is collinear")
                        //print (ni,"next i")
                        if offDists.[pi] <> offDists.[saveIdx (ni-1) distsNeeded] then
                            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.OffsetPoints: can't fix collinear at index %d with index %d and %d because offset distances are mismatching: %f, %f" i pi ni offDists.[pi] offDists.[saveIdx (ni-1) distsNeeded]
                        Pts.[i] <- points.[i] + (nv + pv)*0.5
                if lastIsFirst then Pts.[lastIndex] <- Pts.[0]
                Pts


        /// Offsets a Polyline in 3D space by finding the local offset in each corner.
        /// Positive distance is offset inwards, negative outwards.
        /// Normal distances define a perpendicular offset at each corner.
        /// Auto detects if given points are from a closed Polyline (first point = last point) and loops them.
        static member OffsetPoints(     points:Point3d IList,
                                        offsetDistance: float,
                                        [<OPT;DEF(0.0)>]normalDistance: float ,
                                        [<OPT;DEF(false)>]loop:bool) :Point3d  ResizeArray  =

            if normalDistance = 0.0 then RhinoScriptSyntax.OffsetPoints(points,[offsetDistance],[]              , loop)
            else                         RhinoScriptSyntax.OffsetPoints(points,[offsetDistance],[normalDistance], loop)





