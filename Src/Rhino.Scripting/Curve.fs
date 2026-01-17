namespace Rhino.Scripting.FSharp

open System
open System.Collections.Generic
open Rhino
open Rhino.Geometry
open Rhino.Scripting


/// This module provides functions to create or manipulate Rhino Curves
/// This module is automatically opened when Rhino.Scripting.FSharp namespace is opened.
/// These type extensions are only visible in F#.
[<AutoOpen>]
module AutoOpenCurve =

  type PolylineCurve with

        /// Gets a lazy seq (= IEnumerable) of the points that make up the Polyline.
        member pl.Points =
            seq { for i = 0 to pl.PointCount - 1 do pl.Point(i) }


  type RhinoScriptSyntax with


    ///<summary>Returns parameter of the point on a curve that is closest to a test point.</summary>
    ///<param name="curveId">(Guid) Identifier of a curve object</param>
    ///<param name="point">(Point3d) Sampling point</param>
    ///<returns>(float) The parameter of the closest point on the curve.</returns>
    static member curveClosestParameter(curveId:Guid) (point:Point3d) : float =
        let curve = RhinoScriptSyntax.CoerceCurve(curveId)
        let t = ref 0.
        let rc = curve.ClosestPoint(point, t)
        if not <| rc then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.curveClosestParameter failed. curveId:'%s'" (pretty curveId)
        !t

    ///<summary>Returns parameter of the point on a curve that is closest to a test point.</summary>
    ///<param name="curve">(Geometry.Curve) A Curve geometry object</param>
    ///<param name="point">(Point3d) Sampling point</param>
    ///<returns>(float) The parameter of the closest point on the curve.</returns>
    static member curveGeoClosestParameter (curve:Curve) (point:Point3d): float =
        let t = ref 0.
        let rc = curve.ClosestPoint(point, t)
        if not <| rc then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.curveGeoClosestParameter failed for point %s" point.Pretty
        !t

    ///<summary>Returns the point on a curve that is closest to a test point.</summary>
    ///<param name="curveId">(Guid) Identifier of a curve object</param>
    ///<param name="point">(Point3d) Sampling point</param>
    ///<returns>(Point3d) The closest point on the curve.</returns>
    static member curveClosestPoint (curveId:Guid) (point:Point3d) : Point3d =
        let curve = RhinoScriptSyntax.CoerceCurve(curveId)
        let rc, t = curve.ClosestPoint(point)
        if not <| rc then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.curveClosestPoint failed. curveId:'%s'" (pretty curveId)
        curve.PointAt(t)

    ///<summary>Returns the point on a curve that is closest to a test point.</summary>
    ///<param name="curve">(Geometry.Curve) A Curve geometry object</param>
    ///<param name="point">(Point3d) Sampling point</param>
    ///<returns>(Point3d) The closest point on the curve.</returns>
    static member curveGeoClosestPoint (curve:Curve) (point:Point3d) : Point3d =
        let rc, t = curve.ClosestPoint(point)
        if not <| rc then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.curveGeoClosestPoint failed for point %s" point.Pretty
        curve.PointAt(t)


    ///<summary>Returns the fillet arc if it fits within three points describing two connected lines (= a polyline). Fails otherwise.</summary>
    ///<param name="prevPt">(Point3d) The first point of polyline</param>
    ///<param name="midPt">(Point3d) The middle point of polyline, that will get the fillet</param>
    ///<param name="nextPt">(Point3d) The last (or third) point of polyline</param>
    ///<param name="radius">(float) The radius of the fillet to attempt to create</param>
    ///<returns>(Arc) An Arc geometry.</returns>
    static member FilletArc  (prevPt:Point3d, midPt:Point3d, nextPt:Point3d, radius:float)  : Arc   =
        let A = prevPt-midPt
        let B = nextPt-midPt
        let uA = A |> Vector3d.unitize
        let uB = B |> Vector3d.unitize
        // calculate trim
        let alphaDouble =
            let dot = uA*uB
            if abs(dot) > 0.999  then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletArc: Can't fillet points that are collinear %s,%s,%s" prevPt.Pretty midPt.Pretty nextPt.Pretty
            acos dot
        let alpha = alphaDouble * 0.5
        let beta  = Math.PI * 0.5 - alpha
        let trim = tan(beta) * radius // the setback distance from intersection
        if trim > A.Length then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletArc: Fillet Radius %g is too big for prev %s and  %s" radius prevPt.Pretty midPt.Pretty
        if trim > B.Length then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletArc: Fillet Radius %g is too big for next %s and  %s" radius nextPt.Pretty midPt.Pretty
        let arcStart =  midPt + uA * trim // still on arc plane
        let arcEnd =    midPt + uB * trim
        Arc(arcStart, - uA , arcEnd)

    ///<summary>Fillet some corners of a polyline.</summary>
    ///<param name="fillets">(IDictionary&lt;int,float&gt;) The index of the corners to fillet and the fillet radius</param>
    ///<param name="polyline">(IList&lt;Point3d&gt;) The polyline as point list</param>
    ///<returns>(PolyCurve) A PolyCurve geometry.</returns>
    static member FilletPolyline (fillets: IDictionary<int,float>, polyline:IList<Point3d>) : PolyCurve =
        for i in fillets.Keys do
            if i < 0 || i >= polyline.Count-1 then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletPolyline: cannot fillet corner %d in polyline of %d points" i polyline.Count

        let closed = RhinoScriptSyntax.Distance(polyline.[0], polyline.[polyline.Count-1] ) < RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        let mutable prevPt = polyline.[0]
        let mutable endPt = polyline.[polyline.Count-1]
        let plc = new PolyCurve()
        if fillets.ContainsKey 0 then
            if closed then
                let arc = RhinoScriptSyntax.FilletArc (polyline.[polyline.Count-1], polyline.[0], polyline.[1], fillets.[0])
                plc.Append arc  |> ignore
                prevPt <- arc.EndPoint
                endPt <- arc.StartPoint
                if fillets.ContainsKey (polyline.Count-1) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletPolyline:Cannot set last and first radius on closed polyline fillet"
            else
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletPolyline: Cannot set radius at index 0 on open polyline"

        for i = 1 to polyline.Count - 2 do
            let pt = polyline.[i]
            if fillets.ContainsKey i then
                let ptn = polyline.[i+1]
                let arc = RhinoScriptSyntax.FilletArc (prevPt, pt, ptn, fillets.[i])
                plc.Append (Line (prevPt,arc.StartPoint)) |> ignore
                plc.Append arc |> ignore
                prevPt <- arc.EndPoint
            else
                plc.Append (Line (prevPt,pt)) |> ignore
                prevPt <- pt

        plc.Append(Line(prevPt,endPt))  |> ignore
        plc


    ///<summary>Returns the needed trimming of two planar surfaces in order to fit a fillet of given radius.
    ///    The lines can be anywhere on the plane (except parallel to axis).</summary>
    ///<param name="radius">(float) Radius of filleting cylinder</param>
    ///<param name="direction">(Vector3d) Direction of filleting cylinder, usually the intersection of the two planes to fillet; this might be the cross product of the two lines, but the lines might also be skew</param>
    ///<param name="lineA">(Line) First line to fillet, must not be perpendicular to direction; the lines might also be skew</param>
    ///<param name="lineB">(Line) Second line to fillet, must not be perpendicular to direction or first line; the lines might also be skew</param>
    ///<returns>(float) The setback distance from the intersection point.</returns>
    static member filletSkewLinesTrims (radius:float) (direction:Vector3d) (lineA:Line) (lineB:Line) : float  =
        let ok,axis =
            let pla = Plane(lineA.From, lineA.Direction, direction)
            let plb = Plane(lineB.From, lineB.Direction, direction)
            Intersect.Intersection.PlanePlane(pla,plb)
        if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletSkewLinesTrims: Can't intersect Planes , are lineA and lineB  parallel?"


        let arcPl = Plane(axis.From,axis.Direction)
        let uA = (lineA.Mid - arcPl.Origin) |> Vector3d.projectToPlane arcPl |> Vector3d.unitize // vector of line A projected in arc plane
        let uB = (lineB.Mid - arcPl.Origin) |> Vector3d.projectToPlane arcPl |> Vector3d.unitize // vector of line B projected in arc plane

        // calculate trim
        let alphaDouble =
            let dot = uA*uB
            if abs(dot) > 0.999  then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletSkewLinesTrims: Can't fillet, lineA and lineB and direction vector are in same plane."
            acos dot
        let alpha = alphaDouble * 0.5
        let beta  = Math.PI * 0.5 - alpha
        tan(beta) * radius // the setback distance from intersection

    ///<summary>Creates a fillet curve between two lines,
    ///    the fillet might be an ellipse or free form
    ///    but it always lies on the surface of a cylinder with the given direction and radius.</summary>
    ///<param name="makeSCurve">(bool) Only relevant if curves are skew: make S-curve if true or kink if false</param>
    ///<param name="radius">(float) Radius of filleting cylinder</param>
    ///<param name="direction">(Vector3d) Direction of filleting cylinder, usually the intersection of the two planes to fillet; this might be the cross product of the two lines, but the lines might also be skew</param>
    ///<param name="lineA">(Line) First line to fillet, must not be perpendicular to direction; the lines might also be skew</param>
    ///<param name="lineB">(Line) Second line to fillet, must not be perpendicular to direction or first line; the lines might also be skew</param>
    ///<returns>(NurbsCurve*Arc*Point3d) Fillet curve geometry, the true fillet arc on cylinder (wrong ends), the point where fillet would be at radius 0 (same plane as arc).</returns>
    static member filletSkewLines makeSCurve (radius:float)  (direction:Vector3d) (lineA:Line) (lineB:Line) : NurbsCurve*Arc*Point3d   =
        let ok,axis =
            let pla = Plane(lineA.From, lineA.Direction, direction)
            let plb = Plane(lineB.From, lineB.Direction, direction)
            Intersect.Intersection.PlanePlane(pla,plb)
        if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletSkewLines: Can't intersect Planes , are lineA and lineB  parallel?"


        let arcPl = Plane(axis.From,axis.Direction)
        let uA = (lineA.Mid - arcPl.Origin) |> Vector3d.projectToPlane arcPl |> Vector3d.unitize // vector of line A projected in arc plane
        let uB = (lineB.Mid - arcPl.Origin) |> Vector3d.projectToPlane arcPl |> Vector3d.unitize // vector of line B projected in arc plane

        // calculate trim
        let alphaDouble =
            let dot = uA*uB
            if abs(dot) > 0.999  then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.FilletSkewLines: Can't fillet, lineA and lineB and direction vector are in same plane."
            acos dot
        let alpha = alphaDouble * 0.5
        let beta  = Math.PI * 0.5 - alpha
        let trim = tan(beta) * radius // the setback distance from intersection

        let arcStart0 =  arcPl.Origin + uA * trim // still on arc plane
        let arcEnd0 =    arcPl.Origin + uB * trim
        let tol = radius * 0.001
        let arcStart =  arcStart0 |> Vector3d.projectToLine lineA direction |> Point3d.snapIfClose tol lineA.From |> Point3d.snapIfClose tol lineA.To
        let arcEnd   =  arcEnd0   |> Vector3d.projectToLine lineB direction |> Point3d.snapIfClose tol lineB.From |> Point3d.snapIfClose tol lineB.To
        let arc = Arc(arcStart0, - uA , arcEnd0)

        if alphaDouble > Math.PI * 0.49999 && not makeSCurve then // fillet bigger than 89.999 degrees, one arc from 3 points
            let miA = Line.intersectInOnePoint lineA axis
            let miB = Line.intersectInOnePoint lineB axis
            let miPt  = (miA + miB) * 0.5 // if lines are skew
            let midWei = sin alpha
            let knots=    [| 0. ; 0. ; 1. ; 1.|]
            let weights = [| 1. ; midWei; 1.|]
            let pts =     [| arcStart; miPt ; arcEnd |]
            RhinoScriptSyntax.CreateNurbsCurve(pts, knots, 2, weights), arc, arcPl.Origin

        else // fillet smaller than 89.999 degrees, two arc from 5 points
            let betaH = beta*0.5
            let trim2 = trim - radius * tan(betaH)
            let ma, mb =
                if makeSCurve then
                    arcPl.Origin + uA * trim2 |> Vector3d.projectToLine lineA direction ,
                    arcPl.Origin + uB * trim2 |> Vector3d.projectToLine lineB direction
                else
                    let miA = Line.intersectInOnePoint lineA axis
                    let miB = Line.intersectInOnePoint lineB axis
                    let miPt  = (miA + miB) * 0.5 // if lines are skew
                    arcPl.Origin + uA * trim2 |> Vector3d.projectToLine (Line(miPt,arcStart)) direction ,
                    arcPl.Origin + uB * trim2 |> Vector3d.projectToLine (Line(miPt,arcEnd  )) direction

            let gamma = Math.PI*0.5 - betaH
            let midw= sin(gamma)
            let knots= [| 0. ; 0. ; 1. ; 1. ; 2. ; 2.|]
            let weights = [|1. ; midw ; 1. ; midw ; 1.|]
            let mid = (ma + mb)*0.5
            let pts = [|arcStart; ma; mid; mb; arcEnd|]
            RhinoScriptSyntax.CreateNurbsCurve(pts, knots, 2, weights), arc, arcPl.Origin





