namespace Rhino.Scripting.FSharp

open Rhino.Scripting
open System
open Rhino
open Rhino.Geometry
open Rhino.Scripting.RhinoScriptingUtils
open UtilRHinoScriptingFSharp

#nowarn "44" // for internal inline constructors and hidden obsolete members for error cases

/// When Rhino.Scripting.FSharp is opened this module will be auto-opened.
/// It only contains extension members for type Point3d.
[<AutoOpen>]
module AutoOpenPnt =

    type Point3d with // copied from Euclid 0.16

        /// To convert a Point3d (as it is used in most other Rhino Geometries) to Point3f (as it is used in Meshes)
        member pt.ToPoint3f = Point3f(float32 pt.X, float32 pt.Y, float32 pt.Z)


        /// Format 3D point into string with nice floating point number formatting of X, Y and Z
        /// But without full type name as in pt.ToString()
        member p.AsString = sprintf "X=%s|Y=%s|Z=%s" (PrettyFormat.float p.X) (PrettyFormat.float p.Y) (PrettyFormat.float p.Z)

        //-----------------------------------------------------------------------------------------------------
        // These static members can't be extension methods to be useful for Array.sum and Array.average :
        //-----------------------------------------------------------------------------------------------------


        /// Returns a boolean indicating whether X, Y and Z are exactly 0.0.
        member inline pt.IsOrigin =
            pt.X = 0.0 && pt.Y = 0.0 && pt.Z= 0.0

        /// Returns a boolean indicating if any of X, Y and Z is not exactly 0.0.
        member inline p.IsNotOrigin =
            p.X <> 0.0 || p.Y <> 0.0 || p.Z <> 0.0

        /// Returns a boolean indicating whether the absolute value of X, Y and Z is each less than the given tolerance.
        member inline pt.IsAlmostOrigin tol =
            abs pt.X < tol && abs pt.Y < tol && abs pt.Z < tol

        /// Returns new 3D point with new X coordinate, Y and Z stay the same.
        member inline pt.WithX x =
            Point3d (x, pt.Y, pt.Z)

        /// Returns a new 3D point with new Y coordinate, X and Z stay the same.
        member inline pt.WithY y =
            Point3d (pt.X, y, pt.Z)

        /// Returns a new 3D point with new Z coordinate, X and Y stay the same.
        member inline pt.WithZ z =
            Point3d (pt.X, pt.Y, z)

        /// Returns the distance between two 3D points.
        member inline p.DistanceTo (b:Point3d) =
            let x = p.X-b.X
            let y = p.Y-b.Y
            let z = p.Z-b.Z
            sqrt(x*x + y*y + z*z)

        /// Returns the squared distance between two 3D points.
        /// This operation is slightly faster than the distance function, and sufficient for many algorithms like finding closest points.
        member inline p.DistanceToSquare (b:Point3d) =
            let x = p.X-b.X
            let y = p.Y-b.Y
            let z = p.Z-b.Z
            x*x + y*y + z*z

        /// Returns the distance from Origin (0, 0, 0)
        member inline pt.DistanceFromOrigin =
            sqrt (pt.X*pt.X + pt.Y*pt.Y + pt.Z*pt.Z)

        /// Returns the squared distance from Origin (0, 0, 0)
        member inline pt.DistanceFromOriginSquare =
            pt.X*pt.X + pt.Y*pt.Y + pt.Z*pt.Z

        /// Returns the projected distance from Origin (0, 0, 0). Ignoring the Z component.
        member inline pt.DistanceInXYFromOrigin =
            sqrt (pt.X*pt.X + pt.Y*pt.Y)

        /// Returns the projected square distance from Origin (0, 0, 0). Ignoring the Z component.
        member inline pt.DistanceInXYFromOriginSquare =
            pt.X*pt.X + pt.Y*pt.Y

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member p.FailedWithDistanceFromOrigin(l) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.WithDistFromOrigin %O is too small to be scaled to length %g." p l

        /// Returns new 3D point with given distance from Origin by scaling it up or down.
        member inline pt.WithDistanceFromOrigin (l:float) =
            let d = pt.DistanceFromOrigin
            if isTooTiny d then pt.FailedWithDistanceFromOrigin l // don't compose error msg directly here to keep inlined code small.
            pt * (l/d)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member p.FailedDirectionDiamondInXYTo(o) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.DirectionDiamondInXYTo failed for too short distance between %O and %O." p o

        /// Returns the Diamond Angle from this point to another point projected in X-Y plane.
        /// The diamond angle is always positive and in the range of 0.0 to 4.0 (for 360 Degrees)
        /// 0.0 = Xaxis, going Counter-Clockwise. Ignoring Z component.
        /// This is the fastest angle computation since it does not use Math.Cos or Math.Sin.
        /// It is useful for radial sorting.
        member inline p.DirectionDiamondInXYTo(o:Point3d) =
            // https://stackoverflow.com/a/14675998/969070
            let x = o.X-p.X
            let y = o.Y-p.Y
            if isTooTiny (abs(x) + abs(y)) then p.FailedDirectionDiamondInXYTo(o) // don't compose error msg directly here to keep inlined code small.
            if y >= 0.0 then
                if x >= 0.0 then
                    y/(x+y)
                else
                    1.0 - x/(-x+y)
            else
                if x < 0.0 then
                    2.0 - y/(-x-y)
                else
                    3.0 + x/(x-y)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member p.FailedAngle2PiInXYTo(o:Point3d) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.Angle2PiInXYTo failed for too short distance between %O and %O." p o


        /// Returns the angle in Radians from this point to another point projected in X-Y plane.
        /// 0.0 = Xaxis, going Counter-Clockwise till two Pi.
        member inline p.Angle2PiInXYTo(o:Point3d) =
            // https://stackoverflow.com/a/14675998/969070
            let x = o.X-p.X
            let y = o.Y-p.Y
            if isTooTiny (abs(x) + abs(y)) then p.FailedAngle2PiInXYTo(o)// don't compose error msg directly here to keep inlined code small.
            let a = Math.Atan2(y, x)
            if a < 0. then  a + twoPi
            else            a

        /// Returns the angle in Degrees from this point to another point projected in X-Y plane.
        /// 0.0 = Xaxis, going Counter-Clockwise till 360.
        member inline p.Angle360InXYTo(o:Point3d) =
            p.Angle2PiInXYTo o |> toDegrees

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member p.FailedClosestPointOnLine(fromPt:Point3d, toPt:Point3d) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.ClosestPointOnLine: Line is too short for fromPt %O to %O and testPt %O" fromPt toPt p

        /// Get closest point on finite line to test point.
        member inline testPt.ClosestPointOnLine(fromPt:Point3d, toPt:Point3d) =
            let dir = testPt - fromPt
            let v   = toPt   - fromPt
            let lenSq = v.LengthSq
            if isTooTinySq(lenSq) then testPt.FailedClosestPointOnLine(fromPt, toPt)
            let dot = Vector3d.dot (v, dir) / lenSq
            if   dot <= 0.0 then  fromPt
            elif dot >= 1.0 then  toPt
            else                 fromPt+dot*v


        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member p.FailedDistanceToLine(fromPt:Point3d, toPt:Point3d) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.DistanceToLine: Line is too short for fromPt %O to %O and %O" fromPt toPt p
        /// Returns the distance between point and finite line segment defined by start and end.
        member inline testPt.DistanceToLine(fromPt:Point3d, toPt:Point3d) =
            let dir = testPt - fromPt
            let v   = toPt   - fromPt
            let lenSq = v.LengthSq
            if isTooTinySq(lenSq) then testPt.FailedDistanceToLine(fromPt, toPt)
            let dot = Vector3d.dot (v, dir) / lenSq
            if   dot <= 0.0 then testPt.DistanceTo   fromPt
            elif dot >= 1.0 then testPt.DistanceTo   toPt
            else                 testPt.DistanceTo   (fromPt + v * dot)



        //----------------------------------------------------------------------------------------------
        //--------------------------  Static Members  --------------------------------------------------
        //----------------------------------------------------------------------------------------------

        /// Checks if two 3D points are equal within tolerance.
        /// Use a tolerance of 0.0 to check for an exact match.
        static member inline equals (tol:float) (a:Point3d) (b:Point3d) =
            abs (a.X-b.X) <= tol &&
            abs (a.Y-b.Y) <= tol &&
            abs (a.Z-b.Z) <= tol

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        static member failedCreateFromMembersXYZ(pt:'T,e:exn) = RhinoScriptingFSharpException.Raise $"Rhino.Scripting.FSharp:Point3d.createFromMembersXYZ: {pt} could not be converted to a Rhino.Scripting.FSharp:Point3d:{Environment.NewLine}{e}" pt e

        /// Accepts any type that has a X, Y and Z (UPPERCASE) member that can be converted to a float.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersXYZ pt =
            let x = ( ^T : (member X : _) pt)
            let y = ( ^T : (member Y : _) pt)
            let z = ( ^T : (member Z : _) pt)
            try Point3d(float x, float y, float z)
            with e -> Point3d.failedCreateFromMembersXYZ(pt,e)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        static member failedCreateFromMembersxyz(pt:'T,e:exn) = RhinoScriptingFSharpException.Raise $"Rhino.Scripting.FSharp:Point3d.createFromMembersxyz: {pt} could not be converted to a Rhino.Scripting.FSharp:Point3d:{Environment.NewLine}{e}"
        /// Accepts any type that has a x, y and z (lowercase) member that can be converted to a float.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersxyz pt =
            let x = ( ^T : (member x : _) pt)
            let y = ( ^T : (member y : _) pt)
            let z = ( ^T : (member z : _) pt)
            try Point3d(float x, float y, float z)
            with e -> Point3d.failedCreateFromMembersxyz(pt,e)


        /// Create 3D point from X, Y and Z components.
        static member inline create (x:float, y:float, z:float) = Point3d(x, y, z)


        /// Project point to World X-Y plane.
        /// Use make2D to convert to 2D point instance.
        static member inline projectToXYPlane (pt:Point3d) = Point3d(pt.X, pt.Y, 0.0)

        /// Sets the X value and return new 3D point.
        static member inline withX x (pt:Point3d) = Point3d(x, pt.Y, pt.Z)

        /// Sets the Y value and return new 3D point.
        static member inline withY y (pt:Point3d) = Point3d(pt.X, y, pt.Z)

        /// Sets the Z value and return new 3D point.
        static member inline withZ z (pt:Point3d) = Point3d(pt.X, pt.Y, z)

        /// Gets the X value of 3D point.
        static member inline getX (pt:Point3d) = pt.X

        /// Gets the Y value of 3D point.
        static member inline getY (pt:Point3d) = pt.Y

        /// Gets the Z value of 3D point.
        static member inline getZ (pt:Point3d) = pt.Z

        /// Adds two 3D points and return new 3D point.
        static member inline add (a:Point3d) (b:Point3d) = a + b

        /// Add a 3D point to a 3D vector and return new 3D point.
        static member inline addVec (v:Vector3d) (a:Point3d) = a + v

        /// Returns the midpoint of two 3D points.
        static member inline midPt (a:Point3d) (b:Point3d) = (a+b) * 0.5

        /// Scale a 3D point by a scalar and return new 3D point.
        static member inline scale (f:float) (pt:Point3d) = pt*f

        /// Move point 3D by vector. Same as Point3d.move.
        static member inline translate (shift:Vector3d) (pt:Point3d) =
            pt + shift

        /// Move point 3D by vector. Same as Point3d.translate.
        static member inline move (shift:Vector3d) (pt:Point3d) =
            pt + shift

        /// Add float to X component of a 3D point and return new 3D point.
        static member inline moveX (x:float) (pt:Point3d) = Point3d (pt.X+x, pt.Y, pt.Z)

        /// Add float to Y component of a 3D point and return new 3D point.
        static member inline moveY (y:float) (pt:Point3d) = Point3d (pt.X, pt.Y+y, pt.Z)

        /// Add float to Z component of a 3D point and return new 3D point.
        static member inline moveZ (z:float) (pt:Point3d) = Point3d (pt.X, pt.Y, pt.Z+z)

        /// Returns the distance between two 3D points.
        static member inline distance (a:Point3d) (b:Point3d) = let v = a-b in sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z)

        /// Returns the horizontal distance between two 3D points(ignoring their Z Value)
        static member inline distanceXY (a:Point3d) (b:Point3d) = let x = a.X-b.X in let y=a.Y-b.Y in sqrt(x*x + y*y)

        /// Returns the squared distance between two 3D points.
        /// This operation is slightly faster than the Point3d.distance function, and sufficient for many algorithms like finding closest points.
        static member inline distanceSq (a:Point3d) (b:Point3d) = let v = a-b in  v.X*v.X + v.Y*v.Y + v.Z*v.Z

        /// Returns the distance from World Origin.
        static member inline distanceFromOrigin (pt:Point3d) = pt.DistanceFromOrigin

        /// Returns the square distance from World Origin.
        static member inline distanceFromOriginSquare (pt:Point3d) = pt.DistanceFromOriginSquare

        /// Returns a new 3D point at a given distance from World Origin by scaling the input.
        static member inline setDistanceFromOrigin f (pt:Point3d) = pt.WithDistanceFromOrigin f

        /// Returns angle between three 3D Points in Radians. Range 0.0 to Pi.
        static member anglePiPts (ptPrev:Point3d, ptThis:Point3d, ptNext:Point3d) =
            Vector3d.anglePi (ptPrev-ptThis) (ptNext-ptThis)

        /// Returns angle between three 3D Points in Degrees. Range 0.0 to 180
        static member angle180Pts (ptPrev:Point3d, ptThis:Point3d, ptNext:Point3d) =
            Point3d.anglePiPts (ptPrev, ptThis, ptNext) |> toDegrees

        /// Returns a (not unitized) bisector vector in the middle direction from ptThis.
        /// Code : (ptPrev-ptThis).Unitized  + (ptNext-ptThis).Unitized
        /// ptPrev * ptThis * ptNext ->   bisector vector
        static member inline bisector (ptPrev:Point3d, ptThis:Point3d, ptNext:Point3d) =
            (ptPrev-ptThis).Unitized  + (ptNext-ptThis).Unitized

        /// For three Points describing a plane return a normal.
        /// If the returned vector has length zero then the points are in one line.
        static member normalOf3Pts (a:Point3d, b:Point3d, c:Point3d) = Vector3d.cross (a-b, c-b)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        static member failedDistPt (fromPt:Point3d, dirPt:Point3d, distance:float) = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.distPt: distance from %O to %O is too small to scale to distance: %g" fromPt dirPt distance

        /// Returns a point that is at a given distance from a 3D point in the direction of another point.
        static member inline distPt (fromPt:Point3d, dirPt:Point3d, distance:float) : Point3d =
            let x = dirPt.X - fromPt.X
            let y = dirPt.Y - fromPt.Y
            let z = dirPt.Z - fromPt.Z
            let len = sqrt(x*x + y*y + z*z)
            if isTooTiny len then Point3d.failedDistPt(fromPt, dirPt, distance)
            let fac = distance / len
            Point3d(fromPt.X + x*fac,
                fromPt.Y + y*fac,
                fromPt.Z + z*fac)


        /// Linearly interpolates between two 3D points.
        /// e.g. rel=0.5 will return the middle point, rel=1.0 the endPoint,
        /// rel=3.0 a point three times the distance beyond the end point.
        /// Same as Point3d.lerp.
        static member inline divPt(fromPt:Point3d, toPt:Point3d, rel:float) : Point3d =
            Point3d(fromPt.X + (toPt.X-fromPt.X)*rel,
                fromPt.Y + (toPt.Y-fromPt.Y)*rel,
                fromPt.Z + (toPt.Z-fromPt.Z)*rel)


        /// Linearly interpolates between two 3D points.
        /// e.g. rel=0.5 will return the middle point, rel=1.0 the endPoint,
        /// rel=3.0 a point three times the distance beyond the end point.
        /// Same as Point3d.divPt.
        static member inline lerp(fromPt:Point3d, toPt:Point3d, rel:float) : Point3d =
            Point3d.divPt(fromPt, toPt, rel)


        /// Returns a point that is at a given Z level,
        /// going from a point in the direction of another point.
        static member inline extendToZLevel (fromPt:Point3d, toPt:Point3d, z:float) =
            let v = toPt - fromPt
            if fromPt.Z < toPt.Z && z < fromPt.Z  then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.extendToZLevel cannot be reached for fromPt:%O toPt:%O z:%g" fromPt toPt z
            if fromPt.Z > toPt.Z && z > fromPt.Z  then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.extendToZLevel cannot be reached for fromPt:%O toPt:%O z:%g" fromPt toPt z
            let dot = abs (v * Vector3d.Zaxis)
            if dot < 0.0001 then  RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.extendToZLevel cannot be reached for fromPt:%O toPt:%O because they are both at the same level. target z:%g " fromPt toPt z
            let diffZ = abs (fromPt.Z - z)
            let fac = diffZ / dot
            fromPt + v * fac

        /// Snaps to a point if it is within the snapDistance.
        /// otherwise returns the original point.
        static member inline snapIfClose (snapDistance) (snapTo:Point3d) (pt:Point3d) =
            let v = snapTo-pt
            if v.Length < snapDistance then snapTo else pt

        /// Snaps the points coordinate to the given precision.
        /// e.g. snap 0.1 Point3d(0.123, 0.456, 0) -> Point3d(0.1, 0.5, 0)
        /// e.g. snap 10  Point3d(3    , 19   , 0) -> Point3d(0  , 20 , 0)
        /// does: (Math.Round (x/precision)) * precision
        static member inline snap (precision) (pt:Point3d) =
            if isTooTiny (precision) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.snap: precision too small or negative %A" precision
            Point3d( (Math.Round (pt.X/precision)) * precision,
                 (Math.Round (pt.Y/precision)) * precision,
                 (Math.Round (pt.Z/precision)) * precision)

        /// Every line has a normal vector in X-Y plane.
        /// Rotated Counter-Clockwise in top view.
        /// The result is unitized.
        /// If line is vertical then Xaxis is returned.
        /// see also : Vector3d.perpendicularVecInXY.
        static member normalOfTwoPointsInXY(fromPt:Point3d, toPt:Point3d) =
            let x = toPt.Y - fromPt.Y
            let y = fromPt.X - toPt.X  // this is the same as: Vector3d.cross v Vector3d.Zaxis
            let len = sqrt(x*x + y*y)
            if isTooTiny len then Vector3d.Xaxis
            else Vector3d(x/len, y/len, 0.0)


        /// Offsets two 3D points by two given distances.
        /// The fist distance (distHor) is applied in in X-Y plane.
        /// The second distance (distNormal) is applied perpendicular to the line (made by the two 3D points)
        /// and perpendicular to the horizontal offset direction.
        /// This is in World.Z direction if both points are at the same Z level.
        /// If points are closer than than 1e-6 units the World.Xaxis is used
        /// as first direction and World Z-axis as second direction.
        static member offsetTwoPt(  fromPt:Point3d,
                                    toPt:Point3d,
                                    distHor:float,
                                    distNormal:float) : Point3d*Point3d=
            let v = toPt - fromPt
            let normHor =
                Vector3d.cross(v, Vector3d.Zaxis)
                |> Vector3d.unitizeOrDefault Vector3d.Xaxis

            let normFree =
                Vector3d.cross(v, normHor)
                |> Vector3d.unitizeOrDefault Vector3d.Zaxis

            let shift = distHor * normHor + distNormal * normFree
            fromPt +  shift, toPt + shift



        // --------------- Rotate 2D and 3D: ----------------------------



        // /// Rotate the 3D point around X-axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member rotateXBy (r:Rotation2D) (p:Point3d) = Point3d (p.X, r.Cos*p.Y - r.Sin*p.Z, r.Sin*p.Y + r.Cos*p.Z)

        // /// Rotate the 3D point around Y-axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member rotateYBy (r:Rotation2D) (p:Point3d) = Point3d (r.Sin*p.Z + r.Cos*p.X, p.Y, r.Cos*p.Z - r.Sin*p.X)

        // /// Rotate the 3D point around Z-axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member rotateZBy (r:Rotation2D) (p:Point3d) = Point3d (r.Cos*p.X - r.Sin*p.Y, r.Sin*p.X + r.Cos*p.Y, p.Z)

        // /// Rotate the 3D point around a center 3D point and a X aligned axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member rotateXwithCenterBy (cen:Point3d) (r:Rotation2D) (pt:Point3d) =
        //     let x = pt.X - cen.X
        //     let y = pt.Y - cen.Y
        //     let z = pt.Z - cen.Z
        //     Point3d  (  x                 + cen.X,
        //             r.Cos*y - r.Sin*z + cen.Y,
        //             r.Sin*y + r.Cos*z + cen.Z)

        // /// Rotate the 3D point around a center point and a Y aligned axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member rotateYwithCenterBy (cen:Point3d) (r:Rotation2D) (pt:Point3d) =
        //     let x = pt.X - cen.X
        //     let y = pt.Y - cen.Y
        //     let z = pt.Z - cen.Z
        //     Point3d (   r.Sin*z + r.Cos*x + cen.X,
        //             y                 + cen.Y,
        //             r.Cos*z - r.Sin*x + cen.Z)

        // /// Rotate the 3D point around a center point and a Z aligned axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member rotateZwithCenterBy (cen:Point3d) (r:Rotation2D) (pt:Point3d) =
        //     let x = pt.X - cen.X
        //     let y = pt.Y - cen.Y
        //     let z = pt.Z - cen.Z
        //     Point3d (   r.Cos*x - r.Sin*y + cen.X,
        //             r.Sin*x + r.Cos*y + cen.Y,
        //             z                 + cen.Z)

        // /// Rotate the 3D point in Degrees around X-axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member inline rotateX (angDegree) (pt:Point3d) =
        //     Point3d.rotateXBy (Rotation2D.createFromDegrees angDegree) pt

        // /// Rotate the 3D point in Degrees around Y-axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member inline rotateY (angDegree) (pt:Point3d) =
        //     Point3d.rotateYBy (Rotation2D.createFromDegrees angDegree) pt

        // /// Rotate the 3D point in Degrees around Z-axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member inline rotateZ (angDegree) (pt:Point3d) =
        //     Point3d.rotateZBy (Rotation2D.createFromDegrees angDegree) pt

        // /// Rotate the 3D point in Degrees around center point and a X aligned axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member inline rotateXwithCenter (cen:Point3d) (angDegree) (pt:Point3d) =
        //     Point3d.rotateXwithCenterBy cen (Rotation2D.createFromDegrees angDegree) pt

        // /// Rotate the 3D point in Degrees around center point and a Y aligned axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member inline rotateYwithCenter (cen:Point3d) (angDegree) (pt:Point3d) =
        //     Point3d.rotateYwithCenterBy cen (Rotation2D.createFromDegrees angDegree) pt

        // /// Rotate the 3D point in Degrees around center point and a Z aligned axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member inline rotateZwithCenter (cen:Point3d) (angDegree) (pt:Point3d) =
        //     Point3d.rotateZwithCenterBy cen (Rotation2D.createFromDegrees angDegree) pt


        /// Returns angle in Degrees at mid point (thisPt).
        static member angleInCorner(prevPt:Point3d, thisPt:Point3d, nextPt:Point3d) =
            let a = prevPt-thisPt
            let b = nextPt-thisPt
            Vector3d.angle180 a b



        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        static member failedProjectedParameter(fromPt:Point3d, v:Vector3d, testPt:Point3d)= RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.projectedParameter: %O is too short for fromPt %O and %O" v fromPt testPt

        /// 'fromPt' point and 'v' vector describe an endless 3D line.
        /// 'testPt' gets projected onto this line.
        /// Returns the parameter (or scaling for vector) on this line of the projection.
        static member inline projectedParameter (fromPt:Point3d, v:Vector3d, testPt:Point3d) =
            let dir = testPt-fromPt
            let lenSq = v.LengthSq
            if isTooTinySq(lenSq) then Point3d.failedProjectedParameter(fromPt, v, testPt)
            Vector3d.dot (v, dir) / lenSq

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        static member failedProjectedParameter(fromPt:Point3d, toPt:Point3d, testPt:Point3d)= RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Point3d.projectedParameter: Line is too short for fromPt %O to %O and %O" fromPt toPt testPt

        /// 'fromPt' point and 'toPt' point describe an endless 3D line.
        /// 'testPt' gets projected onto this line.
        /// Returns the parameter (or scaling for vector) on this line of the projection.
        static member inline projectedParameter (fromPt:Point3d, toPt:Point3d, testPt:Point3d) =
            let dir = testPt - fromPt
            let v   = toPt   - fromPt
            let lenSq = v.LengthSq
            if isTooTinySq(lenSq) then Point3d.failedProjectedParameter(fromPt, toPt, testPt)
            Vector3d.dot (v, dir) / lenSq

        /// Finds the inner offset point in a corner ( defined by a Polyline from 3 points ( prevPt, thisPt and nextPt)
        /// The offset from first and second segment are given separately and can vary (prevDist and nextDist).
        /// Use negative distance for outer offset
        /// The orientation parameter is only approximate, it might flip the output normal, so that the  dot-product is positive.
        /// Returns a Value tuple of :
        ///   - the first segment offset vector in actual length  ,
        ///   - second segment offset vector,
        ///   - the offset corner,
        ///   - and the unitized normal at the corner. flipped if needed to match orientation of the orientation input vector (positive dot product)
        /// If Points are  collinear returns: Vector3d.Zero, Vector3d.Zero, Point3d.Origin, Vector3d.Zero
        static member inline findOffsetCorner( prevPt:Point3d,
                                thisPt:Point3d,
                                nextPt:Point3d,
                                prevDist:float,
                                nextDist:float,
                                orientation:Vector3d) : struct(Vector3d * Vector3d * Point3d * Vector3d) =
            let vp = prevPt - thisPt
            let vn = nextPt - thisPt
            // Check if vectors are nearly parallel (angle less than 0.25 degrees)
            // Use cross product magnitude relative to input lengths as parallelism test
            let cross = Vector3d.CrossProduct(vp, vn)
            let crossLenSq = cross.LengthSq
            let vpLenSq = vp.LengthSq
            let vnLenSq = vn.LengthSq
            // sin(0.25°)² ≈ 1.9e-5, so cross²/(|vp|²|vn|²) < 1e-5 means angle < ~0.18°
            if isTooTinySq(vpLenSq) || isTooTinySq(vnLenSq) || crossLenSq < vpLenSq * vnLenSq * 1e-5 then
                struct(Vector3d.Zero, Vector3d.Zero, Point3d.Origin, Vector3d.Zero)
            else
                let n =
                    Vector3d.CrossProduct(vp, vn)
                    |> Vector3d.unitize
                    |> Vector3d.matchOrientation orientation

                let sp = Vector3d.CrossProduct(vp, n) |> Vector3d.withLength prevDist
                let sn = Vector3d.CrossProduct(n, vn) |> Vector3d.withLength nextDist
                let lp = Line(thisPt + sp , vp)  //|>! ( RhinoScriptSyntax.Doc.Objects.AddLine>>ignore)
                let ln = Line(thisPt + sn , vn)  //|>! ( RhinoScriptSyntax.Doc.Objects.AddLine>> ignore)
                let ok, tp , _ = Intersect.Intersection.LineLine(lp, ln) //could also be solved with trigonometry functions
                if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.RhPnt.findOffsetCorner: Intersect.Intersection.LineLine failed on %s and %s" lp.Pretty ln.Pretty
                struct(sp, sn, lp.PointAt(tp), n)  //or ln.PointAt(tn), should be same



    type Point3f with

        /// To convert a Point3f (as it is used in Meshes) to Point3d (as it is used in most other Rhino Geometries)
        member pt.ToPoint3d = Point3d(pt)

        /// Accepts any type that has a X, Y and Z (UPPERCASE) member that can be converted to a float32.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersXYZ pt  =
            let x = ( ^T : (member X : _) pt)
            let y = ( ^T : (member Y : _) pt)
            let z = ( ^T : (member Z : _) pt)
            try Point3f(float32 x, float32 y, float32 z)
            with e -> RhinoScriptingFSharpException.Raise $"Point3f.createFromMembersXYZ: Rhino.Scripting.FSharp:Point3f:{Environment.NewLine}{e}"


        /// Accepts any type that has a x, y and z (lowercase) member that can be converted to a float32.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersxyz pt  =
            let x = ( ^T : (member x : _) pt)
            let y = ( ^T : (member y : _) pt)
            let z = ( ^T : (member z : _) pt)
            try Point3f(float32 x, float32 y, float32 z)
            with e ->  RhinoScriptingFSharpException.Raise $"Point3f.createFromMembersxyz: Rhino.Scripting.FSharp:Point3f:{Environment.NewLine}{e}"