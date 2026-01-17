namespace Rhino.Scripting.FSharp

open Rhino
open Rhino.Geometry
open Rhino.Scripting
open Rhino.Scripting.RhinoScriptingUtils


/// This module provides functions to create or manipulate Rhino Meshes
/// This module is automatically opened when Rhino.Scripting.FSharp namespace is opened.
/// These type extensions are only visible in F#.
[<AutoOpen>]
module AutoOpenMesh =

    type RhinoScriptSyntax with

        /// Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces. Sort points counterclockwise.
        static member MeshAddTriaFace (m:Mesh, a:Point3f, b:Point3f, c:Point3f)  =
            m.Faces.AddFace(
                m.Vertices.Add a ,
                m.Vertices.Add b ,
                m.Vertices.Add c ) |>ignore

        /// Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces. Sort points counterclockwise.
        static member MeshAddTriaFace (m:Mesh, a:Point3d, b:Point3d, c:Point3d)  =
            m.Faces.AddFace(
                m.Vertices.Add (a.X,a.Y,a.Z) ,
                m.Vertices.Add (b.X,b.Y,b.Z) ,
                m.Vertices.Add (c.X,c.Y,c.Z) ) |>ignore


        /// Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces. Sort points counterclockwise.
        static member MeshAddQuadFace (m:Mesh, a:Point3f, b:Point3f, c:Point3f, d:Point3f) =
            m.Faces.AddFace(    m.Vertices.Add a ,
                                m.Vertices.Add b ,
                                m.Vertices.Add c ,
                                m.Vertices.Add d ) |>ignore

        /// Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces. Sort points counterclockwise.
        static member MeshAddQuadFace (m:Mesh, a:Point3d, b:Point3d, c:Point3d, d:Point3d) =
            m.Faces.AddFace(    m.Vertices.Add (a.X,a.Y,a.Z) ,
                                m.Vertices.Add (b.X,b.Y,b.Z) ,
                                m.Vertices.Add (c.X,c.Y,c.Z) ,
                                m.Vertices.Add (d.X,d.Y,d.Z) ) |>ignore

        /// Adds a quad face from two lines. Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces.
        static member MeshAddQuadFace ((m:Mesh), l:Line, ll:Line) =
            RhinoScriptSyntax.MeshAddQuadFace(m, l.From ,l.To ,ll.From , ll.To)


        /// Appends a welded quad to last 2 vertices. Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces.
        static member MeshAddQuadFaceToLastTwo (m:Mesh, a:Point3d, b:Point3d) =
            let c = m.Vertices.Count
            if c<2 then RhinoScriptingFSharpException.Raise "RhinoScriptSyntax.MeshAddQuadFaceToLastTwo: Cannot append quad to mesh with less than 2 vertices. Mesh: %A" m
            else m.Faces.AddFace(c-1, c-2,  m.Vertices.Add (b.X,b.Y,b.Z), m.Vertices.Add (a.X,a.Y,a.Z)) |>ignore

        /// Appends a welded quad to last 2 vertices. Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces.
        static member MeshAddQuadFaceToLastTwo (m:Mesh, l:Line) =
            RhinoScriptSyntax.MeshAddQuadFaceToLastTwo (m, l.From ,l.To)


        /// Adds a welded quad and triangle face to simulate a pentagon. Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces.
        /// Obsolete? Use built-in Ngons instead?
        static member MeshAddPentaFace (m:Mesh, a:Point3d, b:Point3d, c:Point3d, d:Point3d, e:Point3d) =
            let a = m.Vertices.Add (a.X,a.Y,a.Z)
            let d = m.Vertices.Add (d.X,d.Y,d.Z)
            m.Faces.AddFace(a, m.Vertices.Add (b.X,b.Y,b.Z),  m.Vertices.Add (c.X,c.Y,c.Z),  d  ) |>ignore
            m.Faces.AddFace( m.Vertices.Add (e.X,e.Y,e.Z),  a, d ) |>ignore


        /// Adds two welded quad faces to simulate a hexagon. Call Mesh.Normals.ComputeNormals() and Mesh.Compact() after adding the faces.
        /// Obsolete? Use built-in Ngons instead?
        static member MeshAddHexaFace (m:Mesh, a:Point3d, b:Point3d, c:Point3d, d:Point3d, e:Point3d, f:Point3d) =
            let a = m.Vertices.Add (a.X,a.Y,a.Z)
            let d = m.Vertices.Add (d.X,d.Y,d.Z)
            m.Faces.AddFace(a, m.Vertices.Add (b.X,b.Y,b.Z),  m.Vertices.Add (c.X,c.Y,c.Z),  d  ) |>ignore
            m.Faces.AddFace( m.Vertices.Add (e.X,e.Y,e.Z),  m.Vertices.Add (f.X,f.Y,f.Z),  a, d ) |>ignore


        /// Makes a closed loop of welded quads from a list of lines. Requires at least 3 lines.
        /// Last line is ignored; it is considered the same as the first one (e.g. coming from closed Polyline).
        static member MeshAddLoopWelded (m:Mesh, lns:ResizeArray<Line>) =
            if lns.Count < 3 then RhinoScriptingFSharpException.Raise "RhinoScriptSyntax.MeshAddLoopWelded: Requires at least 3 lines, but got %d" lns.Count
            // add first face
            let ln0 = lns.[0]
            let s0 = ln0.From
            let e0 = ln0.To
            let ln1 = lns.[1]
            let s1 = ln1.From
            let e1 = ln1.To
            let mutable a = m.Vertices.Add (s0.X,s0.Y,s0.Z)
            let mutable b = m.Vertices.Add (e0.X,e0.Y,e0.Z)
            let mutable c = m.Vertices.Add (e1.X,e1.Y,e1.Z)
            let mutable d = m.Vertices.Add (s1.X,s1.Y,s1.Z)
            m.Faces.AddFace(a,b,c,d) |> ignore
            let a0,b0 = a,b // save for last face
            // add other faces:
            for i=2 to lns.Count-2 do // ignore last because it is the same as the first point
                let ln = lns.[i]
                let s = ln.From
                let e = ln.To
                a <- d
                b <- c
                c <- m.Vertices.Add (e.X,e.Y,e.Z)
                d <- m.Vertices.Add (s.X,s.Y,s.Z)
                m.Faces.AddFace(a,b,c,d) |> ignore
            //last face
            m.Faces.AddFace(d,c,b0,a0) |> ignore

        /// Makes a closed loop of unwelded quads from a list of lines. Requires at least 2 lines.
        /// Uses all lines with wrapping (last line connects back to first).
        static member MeshAddLoopUnWelded (m:Mesh, lns:ResizeArray<Line>) =
            if lns.Count < 2 then RhinoScriptingFSharpException.Raise "RhinoScriptSyntax.MeshAddLoopUnWelded: Requires at least 2 lines, but got %d" lns.Count
            for i = 0 to lns.Count-1 do
                let lnP = lns.[i]
                let ln = lns.[(i+1)%lns.Count]
                let sP = lnP.From
                let eP = lnP.To
                let s = ln.From
                let e = ln.To
                let a = m.Vertices.Add (sP.X,sP.Y,sP.Z)
                let b = m.Vertices.Add (eP.X,eP.Y,eP.Z)
                let c = m.Vertices.Add (e.X,e.Y,e.Z)
                let d = m.Vertices.Add (s.X,s.Y,s.Z)
                m.Faces.AddFace(a,b,c,d) |> ignore

