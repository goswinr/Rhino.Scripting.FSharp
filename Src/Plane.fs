namespace Rhino.Scripting.FSharp

open System
open Rhino
open Rhino.Geometry
open Rhino.Scripting
open UtilRHinoScriptingFSharp
open Rhino.Scripting.RhinoScriptingUtils



/// When Rhino.Scripting.FSharp is opened this module will be auto-opened.
/// It only contains extension members for type Plane.
[<AutoOpen>]
module AutoOpenPlane=

  type Plane with  // copied from Euclid 0.16

    /// Returns signed distance of point to plane, also indicating on which side it is.
    member inline pl.DistanceToPt pt = pl.ZAxis * (pt-pl.Origin)

    /// Returns the closest point on the plane from a test point.
    member inline pl.ClosestPoint pt = pt - pl.ZAxis*(pl.DistanceToPt pt)

    /// Returns the X, Y and Z parameters of a point with regards to the plane.
    member inline pl.PointParameters pt =
        let v = pt-pl.Origin
        pl.XAxis * v, pl.YAxis * v, pl.ZAxis * v

    /// First finds the closest point on plane from a test point.
    /// Then returns a new plane with Origin at this point and the same Axes vectors.
    member inline pl.PlaneAtClPt pt =
        let o = pl.ClosestPoint pt
        Plane(o, pl.XAxis, pl.YAxis)

    /// Returns the angle to another Plane in Degrees, ignoring the normal's orientation.
    /// So 0.0 if the planes are parallel. And 90 degrees if the planes are perpendicular to each other.
    member inline this.Angle90ToPlane (pl:Plane) =
        Vector3d.angle90 this.ZAxis pl.ZAxis

    /// Returns the angle to a 3D vector in Degrees, ignoring the plane's orientation.
    /// So 0.0 if the vector is parallel to the Plane. And 90 degrees if the vector is perpendicular to the plane.
    /// Fails if the vector is too short (IsZeroLength shorter than 1e-12).
    member inline pl.Angle90ToVec (v:Vector3d) =
        let len = v.Length
        if isTooTiny len then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.Angle90ToVec: Vector is too short: %O" v
        let u = Vector3d(v.X/len, v.Y/len, v.Z/len)
        90.0 - Vector3d.angle90 u pl.ZAxis


    /// Returns the angle to a Line in Degrees, ignoring the ZAxis's orientation.
    /// So 0.0 if the line is parallel to the Plane. And 90 degrees if the line is perpendicular to the plane.
    member inline pl.Angle90ToLine (ln:Line) =
        let x = ln.ToX-ln.FromX
        let y = ln.ToY-ln.FromY
        let z = ln.ToZ-ln.FromZ
        let l = sqrt(x * x  + y * y + z * z)
        if isTooTiny l then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.Angle90ToLine: Line is too short. %O" ln
        let u = Vector3d (x/ l, y/ l, z/ l)
        90.0 - Vector3d.angle90 u pl.ZAxis

    /// Evaluate at 3D parameter.
    member inline p.EvaluateAt (px:float, py:float, pz:float) = p.Origin + p.XAxis*px + p.YAxis*py + p.ZAxis*pz

    /// Evaluate at 2D parameter (Z parameter = 0.0)
    member inline p.EvaluateAtXY (px:float, py:float) = p.Origin + p.XAxis*px + p.YAxis*py

    /// Checks if two Planes are coincident within the distance tolerance. 1e-6 by default.
    /// This means that their Z-axes are parallel within the angle tolerance
    /// and the distance of second origin to the first plane is less than the distance tolerance.
    /// The default angle tolerance is 0.25 degrees.
    /// This tolerance can be customized by an optional minimum cosine value.
    /// See Rhino.Scripting.FSharp:.Cosine module.
    member inline pl.IsCoincidentTo (other:Plane,
                                    [<OPT;DEF(1e-6)>] distanceTolerance:float,
                                    [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine>) =
        pl.ZAxis.IsParallelTo(other.ZAxis, minCosine)
        &&
        abs(pl.DistanceToPt other.Origin) < distanceTolerance


    /// Return a new plane with given Origin.
    [<Obsolete("Use Plane.setOrigin instead")>]
    static member inline setOrig (pt:Point3d) (pl:Plane) =
        Plane(pt, pl.XAxis, pl.YAxis)

    /// Return a new plane with given Origin X value changed.
    [<Obsolete("Use Plane.setOriginX instead")>]
    static member inline setOrigX (x:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withX x, pl.XAxis, pl.YAxis)

    /// Return a new plane with given Origin Y value changed.
    [<Obsolete("Use Plane.setOriginY instead")>]
    static member inline setOrigY (y:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withY y, pl.XAxis, pl.YAxis)

    /// Return a new plane with given Origin Z value changed.
    [<Obsolete("Use Plane.setOriginZ instead")>]
    static member inline setOrigZ (z:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withZ z, pl.XAxis, pl.YAxis)

    /// Return a new plane with Origin translated by Vector3d.
    static member inline translateBy (v:Vector3d) (pl:Plane) =
        Plane(pl.Origin + v, pl.XAxis, pl.YAxis)

    /// Return a new plane with Origin translated in World X direction.
    static member inline translateByWorldX (x:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.moveX x, pl.XAxis, pl.YAxis)

    /// Return a new plane with Origin translated in World Y direction.
    static member inline translateByWorldY (y:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.moveY y, pl.XAxis, pl.YAxis)

    /// Return a new plane with Origin translated in World Z direction.
    static member inline translateByWorldZ (z:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.moveZ z, pl.XAxis, pl.YAxis)


    /// Rotate about Z axis by angle in degree.
    /// Counter clockwise in top view (for WorldXY Plane).
    static member inline rotateZ (angDegree:float) (pl:Plane) =
        let mutable p = pl.Clone()
        if not <| p.Rotate(toRadians angDegree, Vector3d.ZAxis) then
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.RhPlane.rotateZ by %s for %s" (PrettyFormat.float angDegree) pl.Pretty
        p


    //----------------------------------------------------------------------------------------------
    //--------------------------  Static Members  --------------------------------------------------
    //----------------------------------------------------------------------------------------------

    /// Checks if two Parametrized Planes are equal within tolerance distance
    /// For the tips of its units vectors and its origin.
    /// Use a tolerance of 0.0 to check for an exact match.
    static member inline equals (tol:float) (a:Plane) (b:Plane) =
        abs (a.Origin.X - b.Origin.X) <= tol &&
        abs (a.Origin.Y - b.Origin.Y) <= tol &&
        abs (a.Origin.Z - b.Origin.Z) <= tol &&
        abs (a.XAxis.X - b.XAxis.X) <= tol &&
        abs (a.XAxis.Y - b.XAxis.Y) <= tol &&
        abs (a.XAxis.Z - b.XAxis.Z) <= tol &&
        abs (a.YAxis.X - b.YAxis.X) <= tol &&
        abs (a.YAxis.Y - b.YAxis.Y) <= tol &&
        abs (a.YAxis.Z - b.YAxis.Z) <= tol //&&
        //abs (a.ZAxis.X - b.ZAxis.X) <= tol &&
        //abs (a.ZAxis.Y - b.ZAxis.Y) <= tol &&
        //abs (a.ZAxis.Z - b.ZAxis.Z) <= tol


    /// Checks if two 3D Parametrized Planes are coincident within the distance tolerance.
    /// This means that the Z-axes are parallel within 0.25 degrees
    /// and the distance of second origin to the first plane is less than the tolerance.
    static member inline areCoincident tol (a:Plane) (b:Plane) =
        a.IsCoincidentTo (b,tol)

    /// Returns the World Coordinate System Plane at World Origin.
    /// X-axis = World X-axis
    /// Y-axis = World Y-axis
    /// Z-axis = World Z-axis
    /// same as Plane.WorldTop
    static member WorldXY =
        Plane.WorldXY

    /// Returns the World Coordinate System Plane at World Origin.
    /// X-axis = World X-axis
    /// Y-axis = World Y-axis
    /// Z-axis = World Z-axis
    /// same as Plane.WorldXY
    static member WorldTop =
        Plane.WorldTop

    /// Returns the Coordinate System Plane of a Front view.
    /// X-axis = World X-axis
    /// Y-axis = World Z-axis
    /// Z-axis = minus World Y-axis
    static member inline WorldFront =
        Plane(Point3d.Origin, Vector3d.XAxis, Vector3d.ZAxis)

    /// Returns the Coordinate System Plane of a Right view.
    /// X-axis = World Y-axis
    /// Y-axis = World Z-axis
    /// Z-axis = World X-axis
    static member inline WorldRight =
        Plane(Point3d.Origin, Vector3d.YAxis, Vector3d.ZAxis)

    /// Returns the Coordinate System Plane of a Left view.
    /// X-axis = minus World Y-axis
    /// Y-axis = World Z-axis
    /// Z-axis = minus World X-axis
    static member inline WorldLeft =
        Plane(Point3d.Origin, -Vector3d.YAxis, Vector3d.ZAxis)

    /// Returns the Coordinate System Plane of a Back view.
    /// X-axis = minus World X-axis
    /// Y-axis = World Z-axis
    /// Z-axis = World Y-axis
    static member inline WorldBack =
        Plane(Point3d.Origin, -Vector3d.XAxis, Vector3d.ZAxis)

    /// Returns the Coordinate System Plane of a Bottom view.
    /// X-axis = World X-axis
    /// Y-axis = minus World Y-axis
    /// Z-axis = minus World Z-axis
    static member inline WorldBottom =
        Plane(Point3d.Origin, Vector3d.XAxis, -Vector3d.YAxis)

    /// WorldXY rotated 180 degrees round Z-axis.
    static member inline WorldMinusXMinusY=
        Plane(Point3d.Origin, -Vector3d.XAxis, -Vector3d.YAxis)

    /// WorldXY rotated 90 degrees round Z-axis Counter-Clockwise from top.
    static member inline WorldYMinusX=
        Plane(Point3d.Origin, Vector3d.YAxis, -Vector3d.XAxis)

    /// WorldXY rotated 270 degrees round Z-axis Counter-Clockwise from top.
    static member inline WorldMinusYX=
        Plane(Point3d.Origin, -Vector3d.YAxis, Vector3d.XAxis)

    /// WorldXY rotated 180 degrees round X-axis, Z points down now.
    static member inline WorldXMinusY=
        Plane(Point3d.Origin, Vector3d.XAxis, -Vector3d.YAxis)

    /// Builds Plane at first point, X-axis to second point,
    /// Y-axis to third point or at least in plane with third point.
    /// Fails if points are closer than 1e-6.
    static member createThreePoints (origin:Point3d) (xPt:Point3d) (yPt:Point3d) =
        let x = xPt-origin
        let y = yPt-origin
        let lx = x.Length
        let ly = y.Length
        if isTooSmall (lx) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createThreePoints the distance between origin %s and xPt %s is too small" origin.AsString xPt.AsString
        if isTooSmall (ly) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createThreePoints the distance between origin %s and yPt %s is too small" origin.AsString yPt.AsString
        let xf = 1./lx
        let yf = 1./ly
        let xu = Vector3d(x.X*xf, x.Y*xf, x.Z*xf)
        let yu = Vector3d(y.X*yf, y.Y*yf, y.Z*yf)
        if xu.IsParallelTo(yu, Cosine.``1.0``) then
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createThreePoints failed. The points are collinear by less than 1.0 degree, origin %s and xPt %s and yPt %s" origin.AsString xPt.AsString yPt.AsString
        let z  = Vector3d.cross (xu, yu)
        let y' = Vector3d.cross (z, x)
        Plane(origin, xu, y'.Unitized)


    /// Creates a Parametrized Plane from a point and vector representing the X-axis.
    /// The resulting Plane will have the X-Axis in direction of X vector.
    /// The X and Y vectors will define the plane and the side that Z will be on.
    /// The given Y vector does not need to be perpendicular to the X vector, just not parallel.
    /// Fails if the vectors are shorter than 1e-5.
    static member createOriginXaxisYaxis (origin:Point3d, xAxis:Vector3d, yAxis:Vector3d) =
        let lx = xAxis.Length
        let ly = yAxis.Length
        if isTooSmall (lx) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginXaxisYaxis the X-axis is too small. origin %s X-Axis %s" origin.AsString xAxis.AsString
        if isTooSmall (ly) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginXaxisYaxis the Y-axis is too small. origin %s Y-Axis %s" origin.AsString yAxis.AsString
        Plane(origin, xAxis, yAxis)
        // let xf = 1./lx
        // let yf = 1./ly
        // let xu = Vector3d(xAxis.X*xf, xAxis.Y*xf, xAxis.Z*xf)
        // let yu = Vector3d(yAxis.X*yf, yAxis.Y*yf, yAxis.Z*yf)
        // Plane.createOriginXaxisYaxis (origin, xu, yu)


    /// Creates a Parametrized Plane from a point and vector representing the normal (or Z-axis).
    /// The X-axis will be found by taking the cross product of the World Z-axis and the given normal (or Z-axis).
    /// This will make the X-axis horizontal.
    /// If this fails because they are coincident, the cross product of the World Y-axis and the given normal (or Z-axis) will be used.
    /// Fails if the vectors are shorter than 1e-5.
    static member createOriginNormal (origin:Point3d, normal:Vector3d) =
        let len = normal.Length
        if isTooSmall (len) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginNormal the Z-axis is too small. origin %s Z-Axis %s" origin.AsString normal.AsString
        let f = 1./len
        let normal =  Vector3d(normal.X*f, normal.Y*f, normal.Z*f)
        if normal.IsParallelTo(Vector3d.ZAxis, Cosine.``0.5``) then
            let y = Vector3d.cross (normal,Vector3d.XAxis)
            let x = Vector3d.cross (y, normal)
            Plane(origin, x.Unitized, y.Unitized)
        else
            let x = Vector3d.cross (Vector3d.ZAxis,normal)
            let y = Vector3d.cross (normal, x)
            Plane(origin, x.Unitized, y.Unitized)


    /// Creates a Parametrized Plane from a point and unit-vector representing the Z-axis.
    /// The given X vector does not need to be perpendicular to the normal vector, just not parallel.
    /// Fails if the vectors are shorter than 1e-5 or normal and X are parallel.
    static member createOriginNormalXaxis (origin:Point3d, normal:Vector3d, xAxis:Vector3d) =
        let lx = xAxis.Length
        let ln = normal.Length
        if isTooSmall (lx) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginNormalXaxis the X-axis is too small. origin %s X-Axis %s" origin.AsString xAxis.AsString
        if isTooSmall (ln) then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginNormalXaxis the normal is too small. origin %s Normal %s" origin.AsString normal.AsString
        let xf = 1./lx
        let nf = 1./ln
        let xu = Vector3d(xAxis.X *xf,  xAxis.Y*xf,  xAxis.Z*xf)
        let nu = Vector3d(normal.X*nf, normal.Y*nf, normal.Z*nf)
        if nu.IsParallelTo(xu, Cosine.``1.0``) then
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.createOriginNormalXaxis failed. The vectors are collinear by less than 1.0 degrees, origin %s and normal %s and xAxis %s" origin.AsString normal.AsString xAxis.AsString
        let y = Vector3d.cross (nu, xu)
        let x = Vector3d.cross (y, nu)
        Plane(origin, x.Unitized, y.Unitized)


    /// Returns a new plane with given Origin.
    static member inline setOrigin (pt:Point3d) (pl:Plane) =
        Plane(pt, pl.XAxis, pl.YAxis)

    /// Returns a new plane with given Origin X value changed.
    static member inline setOriginX (x:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withX x, pl.XAxis, pl.YAxis)

    /// Returns a new plane with given Origin Y value changed.
    static member inline setOriginY (y:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withY y, pl.XAxis, pl.YAxis)

    /// Returns a new plane with given Origin Z value changed.
    static member inline setOriginZ (z:float) (pl:Plane) =
        Plane(pl.Origin |> Point3d.withZ z, pl.XAxis, pl.YAxis)

    // /// Returns a new plane with Origin translated by Vector3d.
    // static member inline translateBy (v:Vector3d) (pl:Plane) =
    //     Plane(pl.Origin + v, pl.XAxis, pl.YAxis)

    // /// Returns a new plane with Origin translated in World X direction.
    // static member inline translateByWorldX (x:float) (pl:Plane) =
    //     Plane(pl.Origin |> Point3d.moveX x, pl.XAxis, pl.YAxis)

    // /// Returns a new plane with Origin translated in World Y direction.
    // static member inline translateByWorldY (y:float) (pl:Plane) =
    //     Plane(pl.Origin |> Point3d.moveY y, pl.XAxis, pl.YAxis)

    // /// Returns a new plane with Origin translated in World Z direction.
    // static member inline translateByWorldZ (z:float) (pl:Plane) =
    //     Plane(pl.Origin |> Point3d.moveZ z, pl.XAxis, pl.YAxis)

    // /// Rotate about Z-axis of the Plane by angle in degree.
    // /// Counter-Clockwise in top view (for WorldXY Plane).
    // static member inline rotateZ (angDegree:float) (pl:Plane) =
    //     let m = RigidMatrix.createRotationAxisCenter (pl.ZAxis, pl.Origin, angDegree)
    //     let x = Vector3d.transformRigid m pl.XAxis
    //     let y = Vector3d.transformRigid m pl.YAxis
    //     Plane(pl.Origin, x, y)

    /// Move Plane along the local X-axis by the given distance.
    static member inline translateX (d:float) (pl:Plane) =
        Plane(pl.Origin + pl.XAxis*d, pl.XAxis, pl.YAxis)

    /// Move Plane along the local Y-axis by the given distance.
    static member inline translateY (d:float) (pl:Plane) =
        Plane(pl.Origin + pl.YAxis*d, pl.XAxis, pl.YAxis)

    /// Move Plane along the local Z-axis by the given distance.
    /// Same as Plane.offset.
    static member inline translateZ (d:float) (pl:Plane) =
        Plane(pl.Origin + pl.ZAxis*d, pl.XAxis, pl.YAxis)

    /// Move Plane along the local Z-axis by the given distance.
    /// Same as Plane.translateZ.
    static member inline offset (d:float) (pl:Plane) =
        Plane(pl.Origin + pl.ZAxis*d, pl.XAxis, pl.YAxis)

    /// Rotate the Plane 180 degrees on its Y-axis.
    /// Called flip because Z-axis points in the opposite direction.
    static member inline flipOnY (pl:Plane) =
        Plane(pl.Origin, -pl.XAxis, pl.YAxis)

    /// Rotate the Plane 180 degrees on its X-axis.
    /// Called flip because Z-axis points in the opposite direction.
    static member inline flipOnX (pl:Plane) =
        Plane(pl.Origin, pl.XAxis, -pl.YAxis)

    /// Rotate the Plane 180 degrees on its Z-axis.
    static member inline rotateOnZ180 (pl:Plane) =
        Plane(pl.Origin, -pl.XAxis, -pl.YAxis)

    // /// Transforms the plane by the given RigidMatrix.
    // /// The returned Plane has orthogonal unit-vectors.
    // static member transform (m:RigidMatrix) (pl:Plane) =
    //     let o = Point3d.transformRigid m pl.Origin
    //     let x = Vector3d.transformRigid m pl.XAxis
    //     let y = Vector3d.transformRigid m pl.YAxis
    //     let z = Vector3d.transformRigid m pl.ZAxis
    //     Plane (o, x, y, z)

    /// Rotate Plane 180 Degrees around Z-axis if the Y-axis orientation does not match World Y (pl.YAxis.Y < 0.0).
    /// To ensure that Y is always positive. For example for showing Text.
    static member inline rotateZ180IfYNegative (pl:Plane) =
        if pl.YAxis.Y < 0.0 then Plane.rotateOnZ180 pl else pl


    /// Returns the line of intersection between two planes.
    /// Returns None if they are parallel or coincident.
    static member intersect (a:Plane) (b:Plane) : Line option=
        let bn = b.ZAxis
        let an = a.ZAxis
        let v = Vector3d.cross (an, bn)
        if isTooSmallSq v.LengthSq then
            // RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.intersect: Planes are parallel or coincident: %O, %O" a b
            None
        else
            let pa = Vector3d.cross(v, an)
            let nenner = pa * bn
            let ao = a.Origin
            let t = ((b.Origin - ao) * bn) / nenner
            let xpt = ao + pa * t
            let l = Line( xpt.X    , xpt.Y    , xpt.Z,
                            xpt.X+v.X, xpt.Y+v.Y, xpt.Z+v.Z)
            Some <| l

    /// Returns the parameter on the line.
    /// The parameter is the intersection point of the infinite Line with the Plane.
    /// Returns None if they are parallel or coincident.
    static member intersectLineParameter  (ln:Line) (pl:Plane) : float option =
        let z = pl.ZAxis
        let nenner = ln.Tangent * z
        if isTooSmall (abs nenner) then
            // RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.intersectLineParameter: Line and Plane are parallel or line has zero length: %O, %O" ln pl
            None
        else
            Some <| ((pl.Origin - ln.From) * z) / nenner


    /// Returns the line parameter and the X and Y parameters on the Plane as tuple (pLn, pPlX, pPlY).
    /// The parameter is the intersection point of the infinite Line with the Plane.
    /// Returns None if they are parallel or coincident.
    static member intersectLineParameters  (ln:Line) (pl:Plane) : option<float*float*float> =
        let z = pl.ZAxis
        let v = ln.Tangent
        let nenner = v * z
        if isTooSmall (abs nenner) then
            // RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:.Plane.intersectLineParameters: Line and Plane are parallel or line has zero length: %O, %O" ln pl
            None
        else
            let t = ((pl.Origin - ln.From) * z) / nenner
            let xpt = ln.From + v * t
            let v = xpt-pl.Origin
            Some <| (t, pl.XAxis * v, pl.YAxis * v)

    /// Returns intersection point of infinite Line with Plane.
    /// Returns None if they are parallel.
    static member intersectLine (ln:Line) (pl:Plane) : Point3d option =
        match Plane.intersectLineParameter ln pl with
        | Some t -> Some (ln.From + ln.Tangent * t)
        | None -> None

    /// Checks if a finite Line intersects with Plane in one point.
    /// Returns false for NaN values or (almost) parallel or coincident lines.
    static member inline doLinePlaneIntersect (ln:Line) (pl:Plane) =
        let nenner = ln.Tangent * pl.ZAxis
        if isTooSmall (abs nenner) then
            false
        else
            let t = ((pl.Origin - ln.From) * pl.ZAxis) / nenner // if nenner is 0.0 then 't' is Infinity
            0. <= t && t <= 1.