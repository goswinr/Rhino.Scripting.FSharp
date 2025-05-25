namespace Rhino.Scripting.FSharp

open Rhino
open Rhino.Geometry
open System
open Rhino.Scripting.RhinoScriptingUtils
open UtilRHinoScriptingFSharp

module internal VecUnitized =

        let inline private vecDist3(ax:float, ay:float, az:float, bx:float, by:float, bz:float) =
            let x = bx-ax
            let y = by-ay
            let z = bz-az
            sqrt(x*x + y*y + z*z)

        /// Returns angle between two 3D unit-vectors in Radians.
        /// Takes vector orientation into account.
        /// Range 0.0 to Pi( = 0 to 180 Degree)
        let inline anglePi (a:Vector3d) (b:Vector3d) =
            // The "straight forward" method of acos(u.v) has large precision
            // issues when the dot product is near +/-1.  This is due to the
            // steep slope of the acos function as we approach +/- 1.  Slight
            // precision errors in the dot product calculation cause large
            // variation in the output value.
            // To avoid this we use an alternative method which finds the
            // angle bisector by (u-v)/2
            // Because u and v and unit-vectors, (u-v)/2 forms a right angle
            // with the angle bisector.  The hypotenuse is 1, therefore
            // 2*asin(|u-v|/2) gives us the angle between u and v.
            // The largest possible value of |u-v| occurs with perpendicular
            // vectors and is sqrt(2)/2 which is well away from extreme slope
            // at +/-1. (See Windows OS Bug 01706299 for details) (form WPF reference source code)
            let dot = a * b
            if -0.98 < dot && dot < 0.98 then // threshold for switching 0.98 ?
                acos dot
            else
                if dot < 0. then Math.PI - 2.0 * asin(vecDist3(-a.X, -a.Y, -a.Z, b.X, b.Y, b.Z) * 0.5)
                else                       2.0 * asin(vecDist3(a.X ,  a.Y,  a.Z, b.X, b.Y, b.Z) * 0.5)

        /// Returns positive angle between two 3D unit-vectors in Radians.
        /// Ignores orientation.
        /// Range 0.0 to Pi/2 ( = 0 to 90 Degree)
        let inline angleHalfPi (a:Vector3d) (b:Vector3d) =
            let dot = a * b
            let dotAbs = abs dot
            if dotAbs < 0.98 then
                acos dotAbs
            else
                if dot < 0. then 2.0 * asin(vecDist3(-a.X, -a.Y, -a.Z, b.X, b.Y, b.Z) * 0.5)
                else             2.0 * asin(vecDist3(a.X ,  a.Y,  a.Z, b.X, b.Y, b.Z) * 0.5)


        /// Returns positive angle between two 3D unit-vectors in Degrees,
        /// Ignores vector orientation.
        /// Range: 0 to 90 Degrees.
        let inline angle90 (a:Vector3d) (b:Vector3d) =
            angleHalfPi a b |>  toDegrees

        /// Returns positive angle between two 3D unit-vectors in Degrees.
        /// Takes vector orientation into account.
        /// Range 0 to 180 Degrees.
        let inline angle180 (a:Vector3d) (b:Vector3d) =
            anglePi a b |>  toDegrees


#nowarn "44" // to skip Obsolete warnings (members just needs to be public for inlining, but should be hidden)

/// When Rhino.Scripting.FSharp is opened this module will be auto-opened.
/// It only contains extension members for type Vector3d.
[<AutoOpen>]
module AutoOpenVector3d =

    type Vector3d with // copied from Euclid 0.16


        /// To convert Vector3d (as it is used in most other Rhino Geometries)
        /// to a Vector3f (as it is used in Mesh normals)
        member v.ToVector3f = Vector3f(float32 v.X, float32 v.Y, float32 v.Z)

        /// Accepts any type that has a X, Y and Z (UPPERCASE) member that can be converted to a float.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersXYZ pt  =
            let x = ( ^T : (member X : _) pt)
            let y = ( ^T : (member Y : _) pt)
            let z = ( ^T : (member Z : _) pt)
            try Vector3d(float x, float y, float z)
            with e -> RhinoScriptingFSharpException.Raise "Vector3d.createFromMembersXYZ: %A could not be converted to a Vector3d:\r\n%A" pt e


        /// Accepts any type that has a x, y and z (lowercase) member that can be converted to a float.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersxyz pt  =
            let x = ( ^T : (member x : _) pt)
            let y = ( ^T : (member y : _) pt)
            let z = ( ^T : (member z : _) pt)
            try Vector3d(float x, float y, float z)
            with e ->  RhinoScriptingFSharpException.Raise "Vector3d.createFromMembersxyz: %A could not be converted to a Vector3d:\r\n%A" pt e

        //[<Extension>]
        //Unitizes the vector , fails if input is of zero length
        //member inline v.UnitizedUnchecked = let f = 1. / sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z) in Vector3d(v.X*f, v.Y*f, v.Z*f)



        member v.AsString =
            sprintf "X=%s|Y=%s|Z=%s" (PrettyFormat.float v.X) (PrettyFormat.float v.Y) (PrettyFormat.float v.Z)

        // /// Returns the length of the 3D vector.
        // member inline v.Length =
        //     sqrt (v.X*v.X + v.Y*v.Y + v.Z*v.Z)

        /// Returns the squared length of the 3D vector.
        /// The square length is faster to calculate and often good enough for use cases such as sorting vectors by length.
        member inline v.LengthSq =
            v.X*v.X + v.Y*v.Y + v.Z*v.Z


        //-----------------------------------------------------------------------------------------------------
        // These static members can't be extension methods to be useful for Array.sum and Array.average :
        //-----------------------------------------------------------------------------------------------------

        /// Returns a boolean indicating whether X, Y and Z are all exactly 0.0.
        member inline v.IsZero =
            v.X = 0.0 && v.Y = 0.0 && v.Z = 0.0

        /// Returns a boolean indicating if any of X, Y and Z is not exactly 0.0.
        member inline v.IsNotZero =
            not v.IsZero

        /// Check if the 3D Vector3d is shorter than the tolerance.
        /// Also checks if any component is a NaN.
        member inline v.IsTiny tol =
            not (v.Length > tol)

        /// Check if the 3D Vector3d square length is shorter than the squared tolerance.
        /// Also checks if any component is a NaN.
        member inline v.IsTinySq tol =
            not (v.LengthSq > tol)

        /// Returns the length of the 3D Vector3d projected into World X-Y plane.
        member inline v.LengthInXY =
            sqrt (v.X*v.X + v.Y*v.Y)

        /// Returns the squared length of the 3D Vector3d projected into World X-Y plane.
        /// The square length is faster to calculate and often good enough for use cases such as sorting Vector3d by length.
        member inline v.LengthSqInXY =
            v.X*v.X + v.Y*v.Y

        /// Returns  a new 3D Vector3d with new X coordinate, Y and Z stay the same.
        member inline v.WithX x =
            Vector3d (x, v.Y, v.Z)

        /// Returns a new 3D Vector3d with new y coordinate, X and Z stay the same.
        member inline v.WithY y =
            Vector3d (v.X, y, v.Z)

        /// Returns a new 3D Vector3d with new z coordinate, X and Y stay the same.
        member inline v.WithZ z =
            Vector3d (v.X, v.Y, z)

        /// Returns a new 3D Vector3d with half the length.
        member inline v.Half =
            Vector3d (v.X*0.5, v.Y*0.5, v.Z*0.5)

        /// Returns a new 3D Vector3d scaled to the desired length.
        /// Same as Vector3d.withLength.
        member inline v.WithLength (desiredLength:float) =
            let l = v.Length
            if isTooTiny l then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.WithLength %g : %O is too small for unitizing, Tolerance:%g" desiredLength v zeroLengthTolerance
            v * (desiredLength / l)

        // A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member v.FailedUnitized() = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp:Vec.Unitized %O is too small for unitizing, Tolerance:%g" v zeroLengthTolerance
        /// Returns a new 3D Vector3d unitized.
        /// Fails with RhinoScriptingFSharpException if the length of the Vector3d is
        /// too small (1e-16) to unitize.
        member inline v.Unitized =
            let l = v.Length
            if isTooTiny l then v.FailedUnitized() // don't compose error msg directly here to keep inlined code small.
            let li = 1. / l
            Vector3d(li*v.X, li*v.Y, li*v.Z)

        // Returns the 3D Vector3d unitized.
        // If the length of the Vector3d is 0.0 an invalid unit-Vector3d is returned.
        // UnitVector3d(0, 0, 0)
        //member inline v.UnitizedUnchecked =
        //    let li = 1. / sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z)
        //    UnitVector3d.createUnchecked(li*v.X, li*v.Y, li*v.Z)

        /// Test if the 3D Vector3d is a unit-Vector3d.
        /// Test if the Vector3d square length is within 6 float steps of 1.0
        /// So between 0.99999964 and 1.000000715.
        member inline v.IsUnit =
            isOne v.LengthSq

        /// Returns a perpendicular horizontal Vector3d. Rotated counterclockwise.
        /// Or Vector3d.Zero if input is vertical.
        /// Just does Vector3d(-v.Y, v.X, 0.0)
        member inline v.PerpendicularInXY =
            Vector3d(-v.Y, v.X, 0)

        /// 90 Degree rotation Counter-Clockwise around Z-axis.
        member inline v.RotateOnZ90CCW =
            Vector3d( -v.Y, v.X, v.Z)

        /// 90 Degree rotation clockwise around Z-axis.
        member inline v.RotateOnZ90CW =
            Vector3d(v.Y, -v.X, v.Z)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member v.FailedDirectionDiamondInXY() = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.DirectionDiamondInXY: input Vector3d is vertical or zero length:%O" v
        /// The diamond angle.
        /// Calculates the proportion of X to Y component.
        /// It is always positive and in the range of 0.0 to 4.0 (for 360 Degrees)
        /// 0.0 = Xaxis, going Counter-Clockwise.
        /// It is the fastest angle calculation since it does not involve Cosine or ArcTangent functions.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.DirectionDiamondInXY =
            // https://stackoverflow.com/a/14675998/969070
            if isTooTiny (abs v.X + abs v.Y) then v.FailedDirectionDiamondInXY()
            if v.Y >= 0.0 then
                if v.X >= 0.0 then
                    v.Y/(v.X+v.Y)
                else
                    1.0 - v.X/(-v.X+v.Y)
            else
                if v.X < 0.0 then
                    2.0 - v.Y/(-v.X-v.Y)
                else
                    3.0 + v.X/(v.X-v.Y)

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member v.FailedDirection2PiInXY() = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.Direction2PiInXY: input Vector3d is zero length or vertical: %O" v
        /// Returns the angle in Radians from X-axis,
        /// Going Counter-Clockwise till two Pi.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.Direction2PiInXY =
            // https://stackoverflow.com/a/14675998/969070
            if isTooTiny (abs v.X + abs v.Y) then v.FailedDirection2PiInXY()
            let a = Math.Atan2(v.Y, v.X)
            if a < 0. then
                a + twoPi
            else
                a

        /// A separate function to compose the error message that does not get inlined.
        [<Obsolete("Not actually obsolete but just hidden. (Needs to be public for inlining of the functions using it.)")>]
        member v.FailedDirectionPiInXY() = RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.DirectionPiInXY: input Vector3d is zero length or vertical: %O" v
        /// Returns the angle in Radians from X-axis,
        /// Ignores orientation.
        /// Range 0.0 to Pi.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.DirectionPiInXY =
            // https://stackoverflow.com/a/14675998/969070
            if isTooTiny (abs v.X + abs v.Y) then v.FailedDirectionPiInXY()
            let a = Math.Atan2(v.Y, v.X)
            if a < 0. then
                a + Math.PI
            else
                a

        /// Returns the angle in Degrees from X-axis.
        /// Going Counter-Clockwise till 360.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.Direction360InXY =
            v.Direction2PiInXY |> toDegrees

        /// Returns the angle in Degrees from X-axis,
        /// Ignores orientation.
        /// Range 0.0 to 180.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.Direction180InXY =
            v.DirectionPiInXY |> toDegrees

        /// Returns positive angle for rotating Counter-Clockwise from this Vector3d to Vector3d 'b' .
        /// In Diamond Angle. Using only proportion of X to Y components.
        /// Range of 0.0 to 4.0 (for 360 Degrees)
        /// It is the fastest angle calculation since it does not involve Cosine or ArcTangent functions.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        member inline v.AngleDiamondInXYTo (b:Vector3d) =
            let r = b.DirectionDiamondInXY - v.DirectionDiamondInXY
            if r >= 0. then  r
            else r + 4.0

        /// Checks if the angle between the two 3D Vector3d is less than 180 degrees.
        /// Calculates the dot product of two 3D Vector3d.
        /// Then checks if it is bigger than 1e-12.
        /// Fails if any of the two Vector3d is shorter than zeroLengthTolerance  (1e-12).
        member inline v.MatchesOrientation (other:Vector3d) =
            if isTooTinySq(v.LengthSq    ) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.MatchesOrientation: Vector3d 'this' is too short: %s. 'other':%s " v.AsString other.AsString
            if isTooTinySq(other.LengthSq) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.MatchesOrientation: Vector3d 'other' is too short: %s. 'this':%s " other.AsString v.AsString
            v * other > 1e-12


        /// Checks if the angle between the two 3D Vector3d is more than 180 degrees.
        /// Calculates the dot product of two 3D Vector3d.
        /// Then checks if it is smaller than minus 1e-12.
        /// Fails if any of the two Vector3d is shorter than zeroLengthTolerance  (1e-12).
        member inline v.IsOppositeOrientation (other:Vector3d) =
            if isTooTinySq(v.LengthSq    ) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsOppositeOrientation: Vector3d 'this' is too short: %s. 'other':%s " v.AsString other.AsString
            if isTooTinySq(other.LengthSq) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsOppositeOrientation: Vector3d 'other' is too short: %s. 'this':%s " other.AsString v.AsString
            v * other < -1e-12


        /// Checks if 3D Vector3d is parallel to the world X axis. Ignoring orientation.
        /// The absolute deviation tolerance along Y and Z axis is 1e-9.
        /// Fails on Vector3d shorter than 1e-6.
        member inline v.IsXAligned =
            let x = abs (v.X)
            let y = abs (v.Y)
            let z = abs (v.Z)
            if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsXAligned cannot not check very tiny Vector3d. (tolerance 1e-6) %A" v
            else y < 1e-9 && z < 1e-9

        /// Checks if 3D Vector3d is parallel to the world Y axis. Ignoring orientation.
        /// The absolute deviation tolerance along X and Z axis is 1e-9.
        /// Fails on Vector3d shorter than 1e-6.
        member inline v.IsYAligned =
            let x = abs (v.X)
            let y = abs (v.Y)
            let z = abs (v.Z)
            if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsYAligned cannot not check very tiny Vector3d. (tolerance 1e-6) %O" v
            else x < 1e-9 && z < 1e-9

        /// Checks if 3D Vector3d is parallel to the world Z axis. Ignoring orientation.
        /// The absolute deviation tolerance along X and Y axis is 1e-9.
        /// Fails on Vector3d shorter than 1e-6.
        /// Same as v.IsVertical
        member inline v.IsZAligned =
            let x = abs (v.X)
            let y = abs (v.Y)
            let z = abs (v.Z)
            if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsZAligned cannot not check very tiny Vector3d. (tolerance 1e-6) %O" v
            else x < 1e-9 && y < 1e-9

        /// Checks if 3D Vector3d is parallel to the world Z axis. Ignoring orientation.
        /// The absolute deviation tolerance along X and Y axis is 1e-9.
        /// Fails on Vector3d shorter than 1e-6.
        /// Same as v.IsZAligned
        member inline v.IsVertical =
            let x = abs (v.X)
            let y = abs (v.Y)
            let z = abs (v.Z)
            if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsVertical cannot not check very tiny Vector3d. (tolerance 1e-6) %O" v
            else x < 1e-9 && y < 1e-9

        /// Checks if 3D Vector3d is horizontal (Z component is almost zero).
        /// The absolute deviation tolerance along Z axis is 1e-9.
        /// Fails on Vector3d shorter than 1e-6.
        member inline v.IsHorizontal =
            let x = abs (v.X)
            let y = abs (v.Y)
            let z = abs (v.Z)
            if isTooSmall (x+y+z) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsHorizontal cannot not check very tiny Vector3d. (tolerance 1e-6) %O" v
            else z < 1e-9

        /// Checks if two 3D Vector3d are parallel.
        /// Ignores the line orientation.
        /// The default angle tolerance is 0.25 degrees.
        /// This tolerance can be customized by an optional minium cosine value.
        /// See Rhino.Scripting.FSharp.Cosine module.
        /// Fails on Vector3d shorter than zeroLengthTolerance (1e-12).
        member inline this.IsParallelTo(other:Vector3d, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
            let sa = this.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsParallelTo: Vector3d 'this' is too short: %s. 'other':%s " this.AsString other.AsString
            let sb = other.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsParallelTo: Vector3d 'other' is too short: %s. 'this':%s " other.AsString this.AsString
            let au = this  * (1.0 / sqrt sa)
            let bu = other * (1.0 / sqrt sb)
            abs(bu * au) > float minCosine // 0.999990480720734 = cosine of 0.25 degrees:


        /// Checks if two 3D Vector3d are parallel.
        /// Takes the line orientation into account too.
        /// The default angle tolerance is 0.25 degrees.
        /// This tolerance can be customized by an optional minium cosine value.
        /// See Rhino.Scripting.FSharp.Cosine module.
        /// Fails on Vector3d shorter than zeroLengthTolerance (1e-12).
        member inline this.IsParallelAndOrientedTo (other:Vector3d, [<OPT;DEF(Cosine.``0.25``)>] minCosine:float<Cosine.cosine> ) =
            let sa = this.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsParallelAndOrientedTo: Vector3d 'this' is too short: %s. 'other':%s " this.AsString other.AsString
            let sb = other.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsParallelAndOrientedTo: Vector3d 'other' is too short: %s. 'this':%s " other.AsString this.AsString
            let au = this  * (1.0 / sqrt sa)
            let bu = other * (1.0 / sqrt sb)
            bu * au > float minCosine // 0.999990480720734 = cosine of 0.25 degrees:


        /// Checks if two 3D Vector3d are perpendicular to each other.
        /// The default angle tolerance is 89.75 to 90.25 degrees.
        /// This tolerance can be customized by an optional minium cosine value.
        /// The default cosine is 0.0043633 ( = 89.75 deg)
        /// See Rhino.Scripting.FSharp.Cosine module.
        /// Fails on Vector3d shorter than zeroLengthTolerance (1e-12).
        member inline this.IsPerpendicularTo (other:Vector3d, [<OPT;DEF(Cosine.``89.75``)>] maxCosine:float<Cosine.cosine> ) =
            let sa = this.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsPerpendicularTo: Vector3d 'this' is too short: %s. 'other':%s " this.AsString other.AsString
            let sb = other.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.IsPerpendicularTo: Vector3d 'other' is too short: %s. 'this':%s " other.AsString this.AsString
            let au = this  * (1.0 / sqrt sa)
            let bu = other * (1.0 / sqrt sb)
            let d = bu * au
            float -maxCosine < d && d  < float maxCosine // = cosine of 98.75 and 90.25 degrees




        //----------------------------------------------------------------------------------------------
        //--------------------------  Static Members  --------------------------------------------------
        //----------------------------------------------------------------------------------------------

        /// Dot product, or scalar product of two 3D vectors.
        /// Returns a float.
        static member inline dot (a:Vector3d, b:Vector3d) =
            a.X * b.X + a.Y * b.Y + a.Z * b.Z

        /// Cross product, of two 3D vectors.
        /// The resulting vector is perpendicular to both input vectors.
        /// Its length is the area of the parallelogram spanned by the input vectors.
        /// Its direction follows the right-hand rule.
        /// A x B = |A| * |B| * sin(angle)
        static member inline cross (a:Vector3d, b:Vector3d) =
            Vector3d (a.Y * b.Z - a.Z * b.Y,
                a.Z * b.X - a.X * b.Z,
                a.X * b.Y - a.Y * b.X)

        /// Returns the World X-axis with length one: Vector3d(1, 0, 0)
        static member inline Xaxis =
            Vector3d(1, 0, 0)

        /// Returns the World Y-axis with length one: Vector3d(0, 1, 0)
        static member inline Yaxis =
            Vector3d(0, 1, 0)

        /// Returns the World Z-axis with length one: Vector3d(0, 0, 1)
        static member inline Zaxis =
            Vector3d(0, 0, 1)

        /// Checks if two 3D Vector3d are equal within tolerance.
        /// Identical Vector3d in opposite directions are not considered equal.
        /// Use a tolerance of 0.0 to check for an exact match.
        static member inline equals (tol:float) (a:Vector3d) (b:Vector3d) =
            abs (a.X-b.X) <= tol &&
            abs (a.Y-b.Y) <= tol &&
            abs (a.Z-b.Z) <= tol

        /// Returns the distance between the tips of two 3D Vector3d.
        static member inline difference (a:Vector3d) (b:Vector3d) = let v = a-b in sqrt(v.X*v.X + v.Y*v.Y + v.Z*v.Z)

        /// Returns the squared distance between the tips of two 3D Vector3d.
        /// This operation is slightly faster than Vector3d.difference and sufficient for many algorithms like finding closest Vector3d.
        static member inline differenceSq (a:Vector3d) (b:Vector3d) = let v = a-b in  v.X*v.X + v.Y*v.Y + v.Z*v.Z


        /// Gets the X part of this 3D Vector3d.
        static member inline getX (v:Vector3d) =
            v.X

        /// Gets the Y part of this 3D Vector3d.
        static member inline getY (v:Vector3d) =
            v.Y

        /// Gets the Z part of this 3D Vector3d.
        static member inline getZ (v:Vector3d) =
            v.Z

        /// Returns new 3D Vector3d with new X value, Y and Z stay the same.
        static member inline withX  x (v:Vector3d) =
            v.WithX x

        /// Returns new 3D Vector3d with new Y value, X and Z stay the same.
        static member inline withY  y (v:Vector3d) =
            v.WithY y

        /// Returns new 3D Vector3d with new z value, X and Y stay the same.
        static member inline withZ z (v:Vector3d) =
            v.WithZ z

        /// Add two 3D Vector3d together. Returns a new 3D Vector3d.
        static member inline add (a:Vector3d) (b:Vector3d) =
            b + a

        /// Multiplies a 3D Vector3d with a scalar, also called scaling a Vector3d.
        /// Returns a new 3D Vector3d.
        static member inline scale (f:float) (v:Vector3d) =
            Vector3d (v.X * f, v.Y * f, v.Z * f)

        /// Returns a new 3D Vector3d scaled to the desired length.
        /// Same as Vector3d.WithLength. Returns a new 3D Vector3d.
        static member inline withLength(desiredLength:float) (v:Vector3d) =
            v.WithLength desiredLength

        /// Add to the X part of this 3D Vector3d together. Returns a new 3D Vector3d.
        static member inline moveX x (v:Vector3d) =
            Vector3d (v.X+x, v.Y, v.Z)

        /// Add to the Y part of this 3D Vector3d together. Returns a new 3D Vector3d.
        static member inline moveY y (v:Vector3d) =
            Vector3d (v.X, v.Y+y, v.Z)

        /// Add to the Z part of this 3D Vector3d together. Returns a new 3D Vector3d.
        static member inline moveZ z (v:Vector3d) =
            Vector3d (v.X, v.Y, v.Z+z)

        /// Check if the 3D Vector3d is shorter than the tolerance.
        /// Also checks if any component is a NaN.
        static member inline isTiny tol (v:Vector3d) =
            not (v.Length > tol)

        /// Check if the 3D Vector3d square length is shorter than the squared tolerance.
        /// Also checks if any component is a NaN.
        static member inline isTinySq tol (v:Vector3d) =
            not (v.LengthSq > tol)

        /// Returns the length of the 3D Vector3d.
        static member inline length (v:Vector3d) =
            v.Length

        /// Returns the squared length of the 3D Vector3d.
        /// The square length is faster to calculate and often good enough for use cases such as sorting Vector3d by length.
        static member inline lengthSq (v:Vector3d) =
            v.LengthSq

        /// Returns a new 3D Vector3d from X, Y and Z parts.
        static member inline create (x:float, y:float, z:float) =
            Vector3d(x, y, z)


        /// Project Vector3d to World X-Y plane.
        /// Use Vc.ofVector3d to convert to a 2D Vector3d.
        static member inline projectToXYPlane (v:Vector3d) =
            Vector3d(v.X, v.Y, 0.0)

        /// Negate or inverse a 3D Vector3d. Returns a new 3D Vector3d.
        /// Same as Vector3d.flip.
        static member inline reverse (v:Vector3d) =
            -v

        /// Negate or inverse a 3D Vector3d. Returns a new 3D Vector3d.
        /// Same as Vector3d.reverse.
        static member inline flip (v:Vector3d) =
            -v

        /// Flips the Vector3d if Z part is smaller than 0.0
        static member inline flipToPointUp (v:Vector3d) =
            if v.Z < 0.0 then -v else v

        /// Returns 3D Vector3d unitized, fails on zero length Vector3d.
        static member inline unitize (v:Vector3d) =
            v.Unitized

        /// Unitize 3D Vector3d, if input Vector3d is shorter than 1e-6 the default unit-Vector3d is returned.
        static member inline unitizeOrDefault (defaultUnitVector3dtor:Vector3d) (v:Vector3d) =
            let l = v.LengthSq
            if l < 1e-12  then  // = sqrt (1e-06)
                defaultUnitVector3dtor
            else
                let f = 1.0 / sqrt(l)
                Vector3d(v.X*f, v.Y*f, v.Z*f)

        /// Returns three Vector3d's Determinant.
        /// This is also the signed volume of the Parallelepiped defined by these three Vector3d.
        /// Also called scalar triple product, mixed product, Box product, or in German: Spatprodukt.
        /// It is defined as the dot product of one of the Vector3d with the cross product of the other two.
        static member inline determinant (u:Vector3d, v:Vector3d, w:Vector3d) =
            u.X*v.Y*w.Z + v.X*w.Y*u.Z + w.X*u.Y*v.Z - w.X*v.Y*u.Z - v.X*u.Y*w.Z - u.X*w.Y*v.Z


        /// Returns positive angle between two 3D Vector3d in Radians.
        /// Takes Vector3d orientation into account,
        /// Range 0.0 to Pi( = 0 to 180 Degree).
        static member anglePi (a:Vector3d) (b:Vector3d) =
            VecUnitized.anglePi a.Unitized b.Unitized

        /// Returns positive angle between two 3D Vector3d in Degrees.
        /// Takes Vector3d orientation into account,
        /// Range 0 to 180 Degrees.
        static member angle180 (a:Vector3d) (b:Vector3d) =
            VecUnitized.angle180 a.Unitized b.Unitized

        /// Returns positive angle between two 3D Vector3d in Radians.
        /// Ignores Vector3d orientation,
        /// Range: 0.0 to Pi/2 ( = 0 to 90 Degrees)
        static member angleHalfPi (a:Vector3d) (b:Vector3d) =
            VecUnitized.angleHalfPi a.Unitized b.Unitized

        /// Returns positive angle between two 3D Vector3d in Degrees.
        /// Ignores Vector3d orientation,
        /// Range: 0 to 90 Degrees.
        static member angle90 (a:Vector3d) (b:Vector3d) =
            VecUnitized.angle90 a.Unitized b.Unitized

        /// Returns positive angle from Vector3d 'a' to Vector3d 'b' projected in X-Y plane.
        /// In Radians.
        /// Considering Counter-Clockwise rotation round the World Zaxis.
        /// Range: 0.0 to 2 Pi ( = 0 to 360 Degrees)
        static member inline angle2PiInXY (a:Vector3d, b:Vector3d) =
            let r = b.Direction2PiInXY  - a.Direction2PiInXY
            if r >= 0. then  r
            else r + twoPi

        /// Returns positive angle of two 3D Vector3d projected in X-Y plane.
        /// In Degrees.
        /// Considering positive rotation round the World Z-axis.
        /// Range: 0 to 360 Degrees.
        static member inline angle360InXY (a:Vector3d, b:Vector3d) =
            Vector3d.angle2PiInXY (a, b) |> toDegrees

        /// Returns positive angle for rotating Counter-Clockwise from Vector3d 'a' to Vector3d 'b' .
        /// In Diamond Angle. Using only proportion of X to Y components.
        /// Range of 0.0 to 4.0 (for 360 Degrees)
        /// It is the fastest angle calculation since it does not involve Cosine or ArcTangent functions.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        static member inline angleDiamondInXY (a:Vector3d, b:Vector3d) = a.AngleDiamondInXYTo(b)

        /// The diamond angle.
        /// Returns positive angle of 3D Vector3d in World X-Y plane.
        /// Calculates the proportion of X to Y component.
        /// It is always positive and in the range of 0.0 to 4.0 (for 360 Degrees)
        /// 0.0 = Xaxis, going Counter-Clockwise.
        /// It is the fastest angle calculation since it does not involve Cosine or ArcTangent functions.
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        static member inline directionDiamondInXY(v:Vector3d) = v.DirectionDiamondInXY

        /// Returns positive angle of 3D Vector3d in World X-Y plane. Counter-Clockwise from X-axis.
        /// In Radians.
        /// Range: 0.0 to 2 Pi ( = 0 to 360 Degrees)
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        static member inline direction2PiInXY (v:Vector3d) = v.Direction2PiInXY

        /// Returns positive angle of 3D Vector3d in World X-Y plane. Counter-Clockwise from X-axis.
        /// In Degree.
        /// Range: 0.0 to 2 Pi ( = 0 to 360 Degrees)
        /// For World X-Y plane. Considers only the X and Y components of the Vector3d.
        static member inline direction360InXY (v:Vector3d) = v.Direction360InXY

        /// Returns a (not unitized) bisector Vector3d in the middle direction.
        /// Code : a.Unitized + b.Unitized.
        static member inline bisector (a:Vector3d) (b:Vector3d) = a.Unitized + b.Unitized

        /// Ensure that the 3D  Vector3d has a positive dot product with given 3D orientation Vector3d.
        static member inline matchOrientation (orientationToMatch:Vector3d) (vec:Vector3d) =
            if orientationToMatch * vec < 0.0 then -vec else vec


        /// Checks if the angle between the two 3D Vector3d is less than 180 degrees.
        /// Calculates the dot product of two 3D Vector3d.
        /// Then checks if it is bigger than 1e-12.
        /// If any of the two Vector3d is zero length returns false.
        static member inline matchesOrientation (v:Vector3d) (other:Vector3d) =
            v.MatchesOrientation other


        /// Checks if the angle between the two 3D Vector3d is more than 180 degrees.
        /// Calculates the dot product of two 3D Vector3d.
        /// Then checks if it is smaller than minus 1e-12.
        /// If any of the two Vector3d is zero length returns false.
        static member inline isOppositeOrientation (v:Vector3d) (other:Vector3d) =
            v.IsOppositeOrientation other


        /// Checks if Angle between two Vector3d is Below 0.25 Degree.
        /// Ignores Vector3d orientation.
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline areParallel (other:Vector3d) (v:Vector3d) = v.IsParallelTo other

        /// Checks if Angle between two Vector3d is less than 0.25 Degree and orientation matches.
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline areParallelAndMatchOrientation (other:Vector3d) (v:Vector3d) = v.IsParallelAndOrientedTo other

        /// Checks if Angle between two Vector3d is between 98.75 and 90.25 Degree.
        /// Ignores Vector3d orientation.
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline arePerpendicular (other:Vector3d) (v:Vector3d) = v.IsPerpendicularTo other


        // Rotate2D:

        /// 90 Degree rotation Counter-Clockwise around Z-axis.
        static member inline rotateOnZ90CCW(v:Vector3d) = Vector3d( -v.Y, v.X, v.Z)

        /// 90 Degree rotation clockwise around Z-axis.
        static member inline rotateOnZ90CW(v:Vector3d) = Vector3d(  v.Y, -v.X, v.Z)

        // /// Rotate the 3D Vector3d around X-axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member rotateXBy (r:Rotation2D) (v:Vector3d) = Vector3d (v.X, r.Cos*v.Y - r.Sin*v.Z, r.Sin*v.Y + r.Cos*v.Z)

        // /// Rotate the 3D Vector3d around Y-axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member rotateYBy (r:Rotation2D) (v:Vector3d) = Vector3d (r.Sin*v.Z + r.Cos*v.X, v.Y, r.Cos*v.Z - r.Sin*v.X)

        // /// Rotate the 3D Vector3d around Z-axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member rotateZBy (r:Rotation2D) (v:Vector3d) = Vector3d (r.Cos*v.X - r.Sin*v.Y, r.Sin*v.X + r.Cos*v.Y, v.Z)

        // /// Rotate the 3D Vector3d in Degrees around X-axis, from Y to Z-axis, Counter Clockwise looking from right.
        // static member inline rotateX (angDegree) (v:Vector3d) =
        //     Vector3d.rotateXBy (Rotation2D.createFromDegrees angDegree) v

        // /// Rotate the 3D Vector3d in Degrees around Y-axis, from Z to X-axis, Counter Clockwise looking from back.
        // static member inline rotateY (angDegree) (v:Vector3d) =
        //     Vector3d.rotateYBy (Rotation2D.createFromDegrees angDegree) v

        // /// Rotate the 3D Vector3d in Degrees around Z-axis, from X to Y-axis, Counter Clockwise looking from top.
        // static member inline rotateZ (angDegree) (v:Vector3d) =
        //     Vector3d.rotateZBy (Rotation2D.createFromDegrees angDegree) v


        /// Linearly interpolates between two Vector3d.
        /// e.g. rel=0.5 will return the middle Vector3d, rel=1.0 the end Vector3d,
        /// rel=1.5 a Vector3d half the distance beyond the end Vector3d.
        static member lerp (start:Vector3d, ende:Vector3d, rel:float) =
            start + rel * (ende - start)

        /// Spherically interpolates between start and end by amount rel (0.0 to 1.0).
        /// The difference between this and linear interpolation (aka, "lerp") is that the Vector3d are treated as directions rather than points in space.
        /// The direction of the returned Vector3d is interpolated by the angle and its magnitude is interpolated between the magnitudes of start and end.
        /// Interpolation continues before and after the range of 0.0 and 0.1
        static member slerp (start:Vector3d, ende:Vector3d, rel:float) =
            // https://en.wikipedia.org/wiki/Slerp
            // implementation tested in Rhino!
            let sLen = start.Length
            let eLen = ende.Length
            if isTooTiny sLen then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.slerp: Can't interpolate from zero length Vector3d:%A" start
            if isTooTiny eLen then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.slerp: Can't interpolate to zero length Vector3d:%A" ende
            let fs = 1.0 / sLen
            let fe = 1.0 / eLen
            let su  = start * fs //unitized start Vector3d
            let eu  = ende  * fe //unitized end   Vector3d
            let dot = su * eu
            if dot > float Cosine.``0.05`` then  // Vector3d are in the same direction interpolate linear only
                Vector3d.lerp(start, ende, rel)
            elif dot < float Cosine.``179.95`` then
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.slerp: Can't interpolate Vector3d in opposite directions:%A" ende
            else
                let ang = acos(dot) // the angel between the two Vector3d
                let perp = eu - su*dot |> Vector3d.unitize // a Vector3d perpendicular to start and in the same plane with ende.
                let theta = ang*rel // the angle part we want for the result
                let theta360 = (theta+twoPi) % twoPi // make sure it is i the range 0.0 to 2 Pi (360 degrees)
                let cosine = cos (theta360)
                let sine   = sqrt(1.0 - cosine*cosine)
                let res =  //unitized result Vector3d
                    if theta360 < Math.PI then  // in the range 0 to 180 degrees, only applicable if rel is beyond 0.0 or 0.1
                        su * cosine + perp * sine
                    else
                        su * cosine - perp * sine
                let lenRel = sLen + rel * (eLen-sLen)
                if lenRel < 0.0 then
                    Vector3d.Zero // otherwise the Vector3d would get flipped and grow again , only applicable if rel is beyond 0.0 or 0.1
                else
                    res * abs lenRel


        /// Returns the Vector3d length projected into X Y Plane.
        /// sqrt(v.X * v.X  + v.Y * v.Y)
        static member inline lengthInXY(v:Vector3d) = sqrt(v.X * v.X  + v.Y * v.Y)

        /// Checks if 3D Vector3d is parallel to the world X axis. Ignoring orientation.
        /// Tolerance is 1e-6.
        /// Fails on Vector3d shorter than 1e-6.
        static member inline isXAligned (v:Vector3d) = v.IsXAligned

        /// Checks if 3D Vector3d is parallel to the world Y axis. Ignoring orientation.
        /// Tolerance is 1e-6.
        /// Fails on Vector3d shorter than 1e-6.
        static member inline isYAligned (v:Vector3d) = v.IsYAligned

        /// Checks if 3D Vector3d is parallel to the world Z axis. Ignoring orientation.
        /// Tolerance is 1e-6.
        /// Fails on Vector3d shorter than 1e-6.
        /// Same as ln.IsVertical
        static member inline isZAligned (v:Vector3d) = v.IsZAligned

        /// Checks if 3D Vector3d is parallel to the world Z axis. Ignoring orientation.
        /// Tolerance is 1e-6.
        /// Fails on Vector3d shorter than 1e-6.
        /// Same as ln.IsZAligned
        static member inline isVertical (v:Vector3d) = v.IsVertical

        /// Checks if line is horizontal (Z component is almost zero).
        /// Tolerance is 1e-6.
        /// Fails on lines shorter than 1e-6.
        static member inline isHorizontal (v:Vector3d) = v.IsHorizontal

        /// Returns positive or negative slope of a Vector3d in Radians.
        /// In relation to X-Y plane.
        /// Range -1.57 to +1.57 Radians.
        static member inline slopeRadians (v:Vector3d) =
            let len2D = sqrt(v.X*v.X + v.Y*v.Y)
            Math.Atan2(v.Z, len2D)

        /// Returns positive or negative slope of a Vector3d in Degrees.
        /// In relation to X-Y plane.
        /// Range -90 to +90 Degrees.
        static member inline slopeDegrees (v:Vector3d) =
            Vector3d.slopeRadians v |> toDegrees

        /// Returns positive or negative slope of a Vector3d in Percent.
        /// In relation to X-Y plane.
        /// 100% = 45 Degrees.
        /// Returns positive (or negative) Infinity if line is vertical or input has length zero.
        static member inline slopePercent (v:Vector3d) =
            //if isTooTiny (abs(v.Z)) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.slopePercent: Can't get Slope from vertical Vector3d %O" v
            let len2D = sqrt(v.X*v.X + v.Y*v.Y)
            100.0 * v.Z / len2D

        /// Reverse Vector3d if Z part is smaller than 0.0
        static member inline orientUp (v:Vector3d) =
            if v.Z < 0.0 then -v else v

        /// Reverse Vector3d if Z part is bigger than 0.0
        static member inline orientDown (v:Vector3d) =
            if v.Z < 0.0 then v else -v

        /// Returns a perpendicular horizontal Vector3d. Rotated counterclockwise.
        /// Just does Vector3d(-v.Y, v.X, 0.0)
        /// On vertical input Vector3d resulting Vector3d if of zero length.
        static member inline perpendicularInXY (v:Vector3d) =
            Vector3d(-v.Y, v.X, 0.0)

        /// Returns a Vector3d that is perpendicular to the given Vector3d and in the same vertical Plane.
        /// Projected into the X-Y plane input and output Vector3d are parallel and of same orientation.
        /// Not of same length, not unitized.
        /// On vertical input Vector3d resulting Vector3d if of zero length.
        static member inline perpendicularInVerticalPlane (v:Vector3d) =
            let hor = Vector3d(v.Y, -v.X, 0.0)
            let r = Vector3d.cross (v, hor)
            if v.Z < 0.0 then -r else r

        // /// Multiplies a Matrix with a 3D Vector3d
        // /// Since a 3D Vector3d represents a direction or an offset in space, but not a location,
        // /// the implicit the 4th dimension is 0.0 so that all translations are ignored. (Homogeneous Vector3d)
        // static member inline transform (m:Matrix) (v:Vector3d) =
        //     v.Transform(m)

        // /// Multiplies (or applies) a RigidMatrix to a 3D Vector3d.
        // /// Since a 3D Vector3d represents a direction or an offset in space, but not a location,
        // /// all translations are ignored. (Homogeneous Vector3d)
        // static member inline transformRigid (m:RigidMatrix) (v:Vector3d) =
        //     v.TransformRigid(m)


        /// Checks if Angle between two Vector3d is less than given Cosine.
        /// Ignores Vector3d orientation. The angle between two Vector3d can be 0 to 90 degrees ignoring their direction.
        /// Use the Rhino.Scripting.FSharp.Cosine module to get some precomputed cosine values
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline isAngle90Below (cosineValue: float<Cosine.cosine>) (a:Vector3d) (b:Vector3d) =
            let sa = a.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle90Below: Vector3d a is too short: %s. Vector3d b:%s " a.AsString b.AsString
            let sb = b.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle90Below: Vector3d b is too short: %s. Vector3d a:%s " b.AsString a.AsString
            let au = a * (1.0 / sqrt sa)
            let bu = b * (1.0 / sqrt sb)
            abs(bu * au) > float cosineValue

        /// Checks if Angle between two Vector3d is more than given Cosine.
        /// Ignores Vector3d orientation. The angle between two Vector3d can be 0 to 90 degrees ignoring their direction.
        /// Use the Rhino.Scripting.FSharp.Cosine module to get some precomputed cosine values.
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline isAngle90Above(cosineValue: float<Cosine.cosine>) (a:Vector3d) (b:Vector3d) =
            let sa = a.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle90Above: Vector3d a is too short: %s. Vector3d b:%s " a.AsString b.AsString
            let sb = b.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle90Above: Vector3d b is too short: %s. Vector3d a:%s " b.AsString a.AsString
            let au = a * (1.0 / sqrt sa)
            let bu = b * (1.0 / sqrt sb)
            abs(bu * au) < float cosineValue


        /// Checks if Angle between two Vector3d is less than given Cosine.
        /// Does not ignore Vector3d orientation. The angle between two Vector3d can be 0 to 180 degrees.
        /// Use the Rhino.Scripting.FSharp.Cosine module to get some precomputed cosine values
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline isAngle180Below (cosineValue: float<Cosine.cosine>) (a:Vector3d) (b:Vector3d) =
            let sa = a.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle180Below: Vector3d a is too short: %s. Vector3d b:%s " a.AsString b.AsString
            let sb = b.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle180Below: Vector3d b is too short: %s. Vector3d a:%s " b.AsString a.AsString
            let au = a * (1.0 / sqrt sa)
            let bu = b * (1.0 / sqrt sb)
            bu * au > float cosineValue

        /// Checks if Angle between two Vector3d is more than given Cosine.
        /// Does not ignore Vector3d orientation. The angle between two Vector3d can be 0 to 180 degrees.
        /// Use the Rhino.Scripting.FSharp.Cosine module to get some precomputed cosine values.
        /// Fails on zero length Vector3d, tolerance 1e-12.
        static member inline isAngle180Above(cosineValue: float<Cosine.cosine>) (a:Vector3d) (b:Vector3d) =
            let sa = a.LengthSq
            if isTooTinySq(sa) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle180Above: Vector3d a is too short: %s. Vector3d b:%s " a.AsString b.AsString
            let sb = b.LengthSq
            if isTooTinySq(sb) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp.Vector3d.isAngle180Above: Vector3d b is too short: %s. Vector3d a:%s " b.AsString a.AsString
            let au = a * (1.0 / sqrt sa)
            let bu = b * (1.0 / sqrt sb)
            bu * au < float cosineValue

        /// Project vector to Plane
        /// Fails if resulting vector is of almost zero length (RhinoMath.SqrtEpsilon)
        static member projectToPlane (pl:Geometry.Plane) (v:Vector3d) =
            let pt = pl.Origin + v
            let clpt = pl.ClosestPoint(pt)
            let r = clpt-pl.Origin
            if r.IsTiny(RhinoMath.SqrtEpsilon) then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.projectToPlane: Cannot projectToPlane for perpendicular vector %A to given plane %A" v pl
            r

        /// Project point onto a finite line in direction of v
        /// Fails if line is missed by tolerance 1e-6
        //and draws debug objects on layer 'Error-projectToLine'
        static member projectToLine (ln:Line) (v:Vector3d) (pt:Point3d) =
            let h = Line(pt,v)
            let ok,tln,th = Intersect.Intersection.LineLine(ln,h)
            if not ok then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.projectToLine: project in direction failed. (are they parallel?)"
            let a = ln.PointAt(tln)
            let b = h.PointAt(th)
            if (a-b).SquareLength > RhinoMath.ZeroTolerance then
                //Scripting.Doc.Objects.AddLine ln   |> RhinoScriptSyntax.setLayer "Error-projectToLine"
                //Scripting.Doc.Objects.AddLine h    |> RhinoScriptSyntax.setLayer "Error-projectToLineDirection"
                //Scripting.Doc.Objects.AddPoint pt  |> RhinoScriptSyntax.setLayer "Error-projectToLineFrom"
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.projectToLine: missed Line by: %g " (a-b).Length
            a


        ///<summary> Intersects two infinite 3D lines.
        /// The lines are defined by a start point and a Vector3d.
        /// 'ValueNone' is returned, if the angle between the Vector3d is less than 0.25 degrees
        /// or any of them is shorter than 1e-6. These tolerances can be adjusted with optional parameters. </summary>
        ///<param name="ptA"> The start point of the first line.</param>
        ///<param name="ptB"> The start point of the second line.</param>
        ///<param name="vA" > The Vector3d of the first line.</param>
        ///<param name="vB" > The Vector3d of the second line.</param>
        ///<param name="tooShortTolerance" > Is an optional length tolerance. 1e-6 by default.
        ///  If one or both Vector3d are shorter than this ValueNone is returned.</param>
        ///<param name="relAngleDiscriminant"> This is an optional tolerance for the internally calculated relative Angle Discriminant.
        /// The default value corresponds to approx 0.25 degree. Below this angle Vector3d are considered parallel.
        /// Use the module Rhino.Scripting.FSharp.RelAngleDiscriminant to set another tolerance here.</param>
        ///<returns> For (almost) zero length or (almost) parallel Vector3d: ValueNone
        /// Else ValueSome with a tuple of the parameters at which the two infinite 2D lines intersect to each other.
        /// The tuple's order corresponds to the input order.</returns>
        static member intersection( ptA:Point3d,
                                    ptB:Point3d,
                                    vA:Vector3d,
                                    vB:Vector3d,
                                    [<OPT;DEF(1e-6)>] tooShortTolerance:float,
                                    [<OPT;DEF(RelAngleDiscriminant.``0.25``)>] relAngleDiscriminant:float<RelAngleDiscriminant.relAngDiscr>
                                    ) : ValueOption<float*float> =
            //https://stackoverflow.com/a/34604574/969070 but DP and DQ are in wrong order !
            let ax = vA.X
            let ay = vA.Y
            let az = vA.Z
            let bx = vB.X
            let by = vB.Y
            let bz = vB.Z
            let a = ax*ax + ay*ay + az*az // square length of A
            let c = bx*bx + by*by + bz*bz // square length of B
            if a < tooShortTolerance * tooShortTolerance then  // Vector3d A too short
                ValueNone
            elif c < tooShortTolerance * tooShortTolerance then  // Vector3d B too short
                ValueNone
            else
                let b = ax*bx + ay*by + az*bz // dot product of both lines
                let ac = a*c // square of square length, never negative
                let bb = b*b // square of square dot product, never negative
                let discriminant = ac - bb // never negative, the dot product cannot be bigger than the two square length multiplied with each other
                let div = ac+bb // never negative
                // getting the relation between the sum and the subtraction gives a good estimate of the angle between the lines
                // see module Rhino.Scripting.FSharp.RelAngleDiscriminant
                let rel = discriminant/div
                if rel < float relAngleDiscriminant then //parallel
                    ValueNone
                else
                    let vx = ptB.X - ptA.X
                    let vy = ptB.Y - ptA.Y
                    let vz = ptB.Z - ptA.Z
                    let e = bx*vx + by*vy + bz*vz
                    let d = ax*vx + ay*vy + az*vz
                    let t = (c * d - b * e) / discriminant
                    let u = (b * d - a * e) / discriminant
                    ValueSome (t, u)


    type Vector3f with

        /// To convert a Vector3f (as it is used in Mesh normals)
        /// to a Vector3d (as it is used in most other Rhino Geometries)
        member v.ToVector3d = Vector3d(v)

        /// Accepts any type that has a X, Y and Z (UPPERCASE) member that can be converted to a float32.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersXYZ pt  =
            let x = ( ^T : (member X : _) pt)
            let y = ( ^T : (member Y : _) pt)
            let z = ( ^T : (member Z : _) pt)
            try Vector3f(float32 x, float32 y, float32 z)
            with e -> RhinoScriptingFSharpException.Raise "Vector3f.createFromMembersXYZ: %A could not be converted to a Vector3f:\r\n%A" pt e


        /// Accepts any type that has a x, y and z (lowercase) member that can be converted to a float32.
        /// Internally this is not using reflection at runtime but F# Statically Resolved Type Parameters at compile time.
        static member inline createFromMembersxyz pt  =
            let x = ( ^T : (member x : _) pt)
            let y = ( ^T : (member y : _) pt)
            let z = ( ^T : (member z : _) pt)
            try Vector3f(float32 x, float32 y, float32 z)
            with e ->  RhinoScriptingFSharpException.Raise "Vector3f.createFromMembersxyz: %A could not be converted to a Vector3f:\r\n%A" pt e
