namespace Rhino.Scripting.FSharp

open System
open Rhino
open Rhino.Geometry
open Rhino.Scripting
open Rhino.Scripting.RhinoScriptingUtils

/// This module provides functions to create or manipulate Rhino Breps/ Polysurface
/// This module is automatically opened when Rhino.Scripting.FSharp namespace is opened.
/// These type extensions are only visible in F#.
[<AutoOpen>]
module AutoOpenBrep =

  type RhinoScriptSyntax with // TODO change to Brep type extensions ??!!

    ///<summary>Creates a Brep in the Shape of a Slotted Hole. Closed with caps. </summary>
    ///<param name="plane">(Plane)Origin = center of hole</param>
    ///<param name="length">(float) total length of slotted hole</param>
    ///<param name="width">(float) width = radius of slotted hole</param>
    ///<param name="height">(float) height of slotted hole volume</param>
    ///<returns>(Brep) Closed Brep Geometry.</returns>
    static member CreateSlottedHoleVolume( plane:Plane, length, width, height) : Brep  =
        if length<width then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SlottedHole: length= %g must be more than width= %g" length width
        let root05  = sqrt 0.5
        let y05 = 0.5 * width
        let x1 =  0.5 * length
        let x05 = x1 - y05
        let knots = [|
            0.0
            0.0
            2.0
            2.0
            2.785398
            2.785398
            3.570796
            3.570796
            5.570796
            5.570796
            6.356194
            6.356194
            7.141593
            7.141593
            |]
        let weights = [|1.0; 1.0; 1.0; root05; 1.0; root05; 1.0; 1.0; 1.0; root05; 1.0; root05; 1.0 |]
        let points = [|
            Point3d(-x05, -y05, 0.0)
            Point3d(0.0,  -y05, 0.0)
            Point3d(x05,  -y05, 0.0)
            Point3d(x1,   -y05, 0.0)
            Point3d(x1,    0.0, 0.0)
            Point3d(x1,    y05, 0.0)
            Point3d(x05,   y05, 0.0)
            Point3d(0.0,   y05, 0.0)
            Point3d(-x05,  y05, 0.0)
            Point3d(-x1,   y05, 0.0)
            Point3d(-x1,   0.0, 0.0)
            Point3d(-x1,  -y05, 0.0)
            Point3d(-x05, -y05, 0.0)
            |]
        use c1 = new NurbsCurve(3, true, 3, 13)
        for i=0 to 12 do c1.Points.[i] <- ControlPoint( points.[i], weights.[i])
        for i=0 to 13 do c1.Knots.[i] <- knots.[i]
        use c2 = new NurbsCurve(3, true, 3, 13)
        for i=0 to 12 do c2.Points.[i] <- ControlPoint( Point3d(points.[i].X, points.[i].Y, height), weights.[i])
        for i=0 to 13 do c2.Knots.[i] <- knots.[i]
        Transform.PlaneToPlane (Plane.WorldXY, plane) |> c1.Transform |> ignore
        Transform.PlaneToPlane (Plane.WorldXY, plane) |> c2.Transform |> ignore
        let rb = Brep.CreateFromLoft( [|c1;c2|], Point3d.Unset, Point3d.Unset, LoftType.Straight, false )
        if isNull rb || rb.Length <> 1  then
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.*** Failed to Create loft part of  SlottedHole , at tolerance %f" RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        rb.[0].CapPlanarHoles( RhinoScriptSyntax.Doc.ModelAbsoluteTolerance)



    ///<summary>Creates a solid Brep in the Shape of a  cylinder. Closed with caps. </summary>
    ///<param name="plane">(Plane) Origin is center of base of cylinder</param>
    ///<param name="diameter">(float) Diameter of cylinder</param>
    ///<param name="length">(float) total length of the screw brep</param>
    ///<returns>(Brep) Brep Geometry.</returns>
    static member CreateCylinder ( plane:Plane, diameter, length) : Brep  =
        let circ = Circle(plane,diameter*0.5)
        let cy = Cylinder(circ,length)
        Brep.CreateFromCylinder(cy, capBottom=true, capTop=true)

    ///<summary>Creates a Brep in the shape of a Countersunk Screw Hole, 45 degrees slope.
    ///    A capped cone and a cylinder. One closed Polysurface.</summary>
    ///<param name="plane">(Plane) Origin is center of cone-base or head</param>
    ///<param name="outerDiameter">(float) Diameter of cone base</param>
    ///<param name="innerDiameter">(float) Diameter of cylinder</param>
    ///<param name="length">(float) Total length of the screw brep</param>
    ///<returns>(Brep) Brep geometry.</returns>
    static member CreateCounterSunkScrewVolume ( plane:Plane, outerDiameter, innerDiameter, length) : Brep  =
        let r = outerDiameter*0.5
        let mutable plco = Plane(plane)
        plco.Origin <- plco.Origin + plco.ZAxis * r
        plco.Flip()
        let cone = Cone(plco, r, r)
        let coneSrf = Brep.CreateFromCone(cone, capBottom=true)
        plane.Rotate(Math.PI * 0.5, plane.ZAxis)|> RhinoScriptingFSharpException.FailIfFalse "rotate plane" // so that seam of cone an cylinder align
        let cySrf = RhinoScriptSyntax.CreateCylinder(plane, innerDiameter, length)
        let bs = Brep.CreateBooleanUnion( [coneSrf; cySrf], RhinoScriptSyntax.Doc.ModelAbsoluteTolerance)
        if bs.Length <> 1 then RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.%d items as result from creating countersunk screw" bs.Length
        let brep = bs.[0]
        if brep.SolidOrientation = BrepSolidOrientation.Inward then brep.Flip()
        brep

    ///If brep.SolidOrientation is inward then flip brep.
    static member OrientBrep (brep:Brep) : Brep  =
        if brep.SolidOrientation = BrepSolidOrientation.Inward then
            brep.Flip()
        brep
    ///<summary>Transforms a planar 2D curve in XY plane to the given plane and then extrudes it with CapPlanarHoles, with optional extensions at both ends.</summary>
    ///<param name="curveToExtrudeInWorldXY">(Curve) A curve in world XY plane</param>
    ///<param name="plane">(Plane) A plane with any orientation</param>
    ///<param name="height">(float) The height to extrude along the Z axis of plane</param>
    ///<param name="extraHeightPerSide">(float) Optional, Default Value: <c>0.0</c>, extra extension of the extrusion on both sides</param>
    ///<returns>(Brep) Brep geometry.</returns>
    static member CreateExtrusionAtPlane(curveToExtrudeInWorldXY:Curve, plane:Plane, height, [<OPT;DEF(0.0)>]extraHeightPerSide:float) : Brep =
        let mutable pl = Plane(plane)
        if extraHeightPerSide <> 0.0 then
            pl.Origin <- pl.Origin - pl.ZAxis*extraHeightPerSide
        let xForm = RhinoScriptSyntax.XformRotation1(Plane.WorldXY,pl)
        let c = curveToExtrudeInWorldXY.DuplicateCurve()
        c.Transform(xForm) |> RhinoScriptingFSharpException.FailIfFalse "xForm in CreateExtrusionAtPlane"
        let h = extraHeightPerSide + height
        let brep = Surface.CreateExtrusion(c, pl.ZAxis * h )
                        .ToBrep()
                        .CapPlanarHoles( RhinoScriptSyntax.Doc.ModelAbsoluteTolerance)
        if brep.SolidOrientation = BrepSolidOrientation.Inward then brep.Flip()
        brep


    ///<summary>Subtracts trimmer from brep (= BooleanDifference),
    /// so that a single brep is returned,
    /// draws objects and zooms on them if an error occurs.</summary>
    ///<param name="trimmer">(Brep) The volume to cut out</param>
    ///<param name="keep">(Brep) The volume to keep</param>
    ///<param name="subtractionLocations">(int) Optional, The number of locations where the brep is expected to be cut
    ///  This is an optional safety check that makes it twice as slow.
    ///  It ensures that the count of breps from Brep.CreateBooleanIntersection is equal to subtractionLocations</param>
    ///<returns>(Brep) Brep geometry.</returns>
    static member SubtractBrep (keep:Brep,trimmer:Brep,[<OPT;DEF(0)>]subtractionLocations:int)  :Brep =
        let draw s b = RhinoScriptSyntax.Ot.AddBrep(b)|> RhinoScriptSyntax.setLayer s

        if not trimmer.IsSolid then
            draw "debug trimmer" trimmer
            RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference trimmer is NOT a closed Polysurface"
        if not keep.IsSolid then
            draw "debug keep" keep
            RhinoScriptSyntax.ZoomBoundingBox(keep.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference keep Volume is NOT a closed Polysurface"

        if subtractionLocations <> 0 then
            let xs = Brep.CreateBooleanIntersection (keep,trimmer, RhinoScriptSyntax.Doc.ModelAbsoluteTolerance) // TODO expensive extra check
            if isNull xs then
                draw "debug trimmer no Intersection" trimmer
                draw "debug keep no Intersection" keep
                RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanIntersection check is null, no intersection found, tolerance = %g" RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
            if xs.Length <> subtractionLocations then
                draw "debug trimer empty Intersection" trimmer
                draw "debug keep empty Intersection" keep
                RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
                RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanIntersection check returned %d breps instead of one , tolerance = %g" xs.Length RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
            for x in xs do x.Dispose()

        let bs =  Brep.CreateBooleanDifference(keep,trimmer, RhinoScriptSyntax.Doc.ModelAbsoluteTolerance)
        if isNull bs then
            draw "debug trimmer" trimmer
            draw "debug keep" keep
            RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference is null, tolerance = %g" RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        if bs.Length = 0 then
            draw "debug trimer for empty result" trimmer
            draw "debug keep for empty result" keep
            RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference returned 0 breps instead of one , tolerance = %g" RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        if bs.Length <> 1 then
            bs |> Seq.iter (draw "debug more than one")
            draw "debug trimer for more than one" trimmer
            RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference returned %d breps instead of one , tolerance = %g" bs.Length RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        let brep = bs.[0]
        if subtractionLocations = 0 && brep.Vertices.Count = keep.Vertices.Count then // extra test if
            draw "debug trimmer same vertex count on  result" trimmer
            draw "debug keep same vertex count on  result" keep
            RhinoScriptSyntax.ZoomBoundingBox(trimmer.GetBoundingBox(false))
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.SubtractBrep:CreateBooleanDifference returned same vertex count on input and output brep is this desired ?, tolerance = %g" RhinoScriptSyntax.Doc.ModelAbsoluteTolerance
        if brep.SolidOrientation = BrepSolidOrientation.Inward then  brep.Flip()
        brep

    ///<summary> Calls Mesh.CreateFromBrep, and Mesh.HealNakedEdges() to try to ensure Mesh is closed if input is closed.</summary>
    ///<param name="brep">(Brep) The Polysurface to extract Mesh from.</param>
    ///<param name="meshingParameters">(MeshingParameters) Optional, The meshing parameters, if omitted the current meshing parameters are used.</param>
    ///<returns>(Result&lt;Mesh,Mesh&gt;) Ok Mesh or Error Mesh if input brep is closed but output Mesh not. Fails if no meshes can be extracted.</returns>
    static member ExtractRenderMesh (brep:Brep,[<OPT;DEF(null:MeshingParameters)>]meshingParameters:MeshingParameters) :Result<Mesh,Mesh> =
        let meshing =
            if notNull meshingParameters then
                meshingParameters
            else
                RhinoScriptSyntax.Doc.GetCurrentMeshingParameters()
        meshing.ClosedObjectPostProcess <- true // not needed use heal instead
        let ms = Mesh.CreateFromBrep(brep,meshing)
        let m = new Mesh()
        for p in ms do
            if notNull p then
                m.Append p
        if m.Vertices.Count < 3 then
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.ExtractRenderMesh: failed to extract a mesh from brep: %A of %d Faces" brep brep.Faces.Count

        //let g = ref Guid.Empty
        if brep.IsSolid && not m.IsClosed then // https://discourse.mcneel.com/t/failed-to-create-closed-mesh-with-mesh-createfrombrep-brep-meshing-params-while-sucessfull-with-rhino-command--mesh/35481/8
            m.HealNakedEdges( RhinoScriptSyntax.Doc.ModelAbsoluteTolerance * 100.0) |> ignore // see https://discourse.mcneel.com/t/mesh-createfrombrep-fails/93926
            if not m.IsClosed then
                m.HealNakedEdges( RhinoScriptSyntax.Doc.ModelAbsoluteTolerance * 1000.0 + meshing.MinimumEdgeLength * 100.0) |> ignore
        if  not m.IsValid then
            //Scripting.Doc.Objects.AddBrep brep|> RhinoScriptSyntax.setLayer "Rhino.Scripting.FSharp: RhinoScriptSyntax.ExtractRenderMesh mesh from Brep invalid"
            RhinoScriptingFSharpException.Raise "Rhino.Scripting.FSharp: RhinoScriptSyntax.ExtractRenderMesh: failed to create valid mesh from brep"
        elif brep.IsSolid && not m.IsClosed then
            Result.Error m
            //Scripting.Doc.Objects.AddMesh m |> RhinoScriptSyntax.setLayer "Rhino.Scripting.FSharp: RhinoScriptSyntax.ExtractRenderMesh not closed"
            //printf "Mesh from closed Brep is not closed, see debug layer"
            //if  m0.IsValid && m0.IsClosed && ( g := RhinoScriptSyntax.Doc.Objects.AddMesh m0 ; !g <> Guid.Empty) then
            //    Ok !g
            //else                        //if it fails it uses ExtractRenderMesh command and returns both mesh and temporary created brep Guid</
            //    let mb = brep |> RhinoScriptSyntax.Doc.Objects.AddBrep
            //    RhinoScriptSyntax.EnableRedraw(true)
            //    RhinoScriptSyntax.Doc.Views.Redraw()
            //    RhinoScriptSyntax.SelectObject(mb)
            //    RhinoScriptSyntax.Command("ExtractRenderMesh ") |> RhinoScriptingFSharpException.FailIfFalse "mesh render"
            //    let ms = RhinoScriptSyntax.LastCreatedObjects()
            //    if ms.Count <> 1 then RhinoScriptingFSharpException.Raise "getRenderMesh: %d in LastCreatedObjects" ms.Count
            //    RhinoScriptSyntax.EnableRedraw(false)
            //    let k = RhinoScriptSyntax.UnselectAllObjects()
            //    if k <> 1 then RhinoScriptingFSharpException.Raise "getRenderMesh: %d Unselected" k
            //    Error (ms.[0],mb)
        else
            Result.Ok m

