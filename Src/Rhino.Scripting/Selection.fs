namespace Rhino.Scripting.FSharp

open System
open System.Collections.Generic

open Rhino
open Rhino.Geometry
open Rhino.Scripting.RhinoScriptingUtils
open Rhino.Scripting

/// This module provides functions similar to Rhino.Scripting.FSharp.GetObject(..)
/// This module is automatically opened when Rhino.Scripting.FSharp namespace is opened.
/// These type extensions are only visible in F#.
[<AutoOpen>]
module AutoOpenSelection =

  // these functions are similar to the ones found in Rhino.Scripting.FSharp: Scripting_Selection.fs


  let internal rememberedObjects = Dict<string,ResizeArray<Guid>>()

  type RhinoScriptSyntax with


    ///<summary>Returns identifiers of all objects in the current model or paper space that are not hidden, not locked nor on turned off layers.</summary>
    ///<param name="filter">(int) Optional, Default Value: <c>0</c>
    ///    The type(s) of geometry (points, Curves, Surfaces, Meshes,...)
    ///    that can be selected. Object types can be added together to filter
    ///    several different kinds of geometry. use the RhinoScriptSyntax.Filter enum to get values, they can be joined with '+'</param>
    ///<param name="printCount">(bool) Optional, Default Value: <c>true</c> Print object count to command window</param>
    ///<param name="includeReferences">(bool) Optional, Default Value: <c>false</c> Include reference objects such as work session objects</param>
    ///<param name="includeLockedObjects">(bool) Optional, Default Value: <c>false</c> Include locked objects</param>
    ///<param name="includeLights">(bool) Optional, Default Value: <c>false</c> Include light objects</param>
    ///<param name="includeGrips">(bool) Optional, Default Value: <c>false</c> Include grips objects</param>
    ///<returns>(Guid ResizeArray) Identifiers for all the objects that are not hidden and who's layer is on and visible.</returns>
    static member ShownObjects(     [<OPT;DEF(0)>]filter:int,
                                    [<OPT;DEF(true)>]printCount:bool,
                                    [<OPT;DEF(false)>]includeReferences:bool,
                                    [<OPT;DEF(false)>]includeLockedObjects:bool,
                                    [<OPT;DEF(false)>]includeLights:bool,
                                    [<OPT;DEF(false)>]includeGrips:bool) : Guid ResizeArray =
        let viewId = // only get object from model space if current or current page
            if RhinoScriptSyntax.Doc.Views.ActiveView :? Display.RhinoPageView then RhinoScriptSyntax.Doc.Views.ActiveView.MainViewport.Id
            else Guid.Empty
        let Vis = new Collections.Generic.HashSet<int>()
        for layer in RhinoScriptSyntax.Doc.Layers do
            if not layer.IsDeleted && layer.IsVisible then
                Vis.Add(layer.Index) |> ignore
        let it = DocObjects.ObjectEnumeratorSettings()
        it.IncludeLights <- includeLights //TODO check what happens to layout objects !!! included ?
        it.IncludeGrips <- includeGrips
        it.NormalObjects <- true
        it.LockedObjects <- includeLockedObjects
        it.HiddenObjects <- false
        it.ReferenceObjects <- includeReferences
        it.ObjectTypeFilter <- ObjectFilterEnum.GetFilterEnum (filter)
        it.DeletedObjects <- false
        //it.VisibleFilter <- true
        let objects = RhinoScriptSyntax.Doc.Objects.GetObjectList(it)
        let objectIds = ResizeArray()
        for ob in objects do
            if ob.Attributes.ViewportId = viewId then // only get object from model space if current or current page
                if Vis.Contains(ob.Attributes.LayerIndex) then
                        objectIds.Add(ob.Id)
        if printCount then
            // RhinoScriptSyntax.PrintfnBlue "ShownObjects found %s"  ( RhinoScriptSyntax.ObjectDescription(objectIds)) // TODO
            RhinoScriptSyntax.Print $"ShownObjects found %d{objectIds.Count} objects"
        objectIds


    ///<summary>Returns the same objects as in the last user interaction with the same prompt message
    /// If none found, Prompts user to pick or select one or more objects and remembers them.
    /// Call rs.ClearRememberedObjects() to clear the memory.</summary>
    ///<param name="message">(string) A prompt or message, should be unique, this will be the key in dictionary to remember objects</param>
    ///<param name="filter">(int) Optional, The type(s) of geometry (points, Curves, Surfaces, Meshes,...)
    ///    that can be selected. Object types can be added together to filter
    ///    several different kinds of geometry. use the RhinoScriptSyntax.Filter enum to get values, they can be joined with '+'</param>
    ///<param name="group">(bool) Optional, Default Value: <c>true</c>
    ///    Honor object grouping. If omitted and the user picks a group,
    ///    the entire group will be picked (True). Note, if filter is set to a
    ///    value other than 0 (All objects), then group selection will be disabled</param>
    ///<param name="preselect">(bool) Optional, Default Value: <c>true</c>
    ///    Allow for the selection of pre-selected objects</param>
    ///<param name="select">(bool) Optional, Default Value: <c>false</c>
    ///    Select the picked objects. If False, the objects that are
    ///    picked are not selected</param>
    ///<param name="objects">(Guid seq) Optional, List of objects that are allowed to be selected. If set customFilter will be ignored</param>
    ///<param name="minimumCount">(int) Optional, Default Value: <c>1</c>
    ///    Minimum count of objects allowed to be selected</param>
    ///<param name="maximumCount">(int) Optional, Default Value: <c>0</c>
    ///    Maximum count of objects allowed to be selected</param>
    ///<param name="printCount">(bool) Optional, Default Value: <c>true</c> Print object count to command window</param>
    ///<param name="customFilter">(Input.Custom.GetObjectGeometryFilter) Optional, Will be ignored if 'objects' are set. Calls a custom function in the script and passes
    ///    the Rhino Object, Geometry, and component index and returns true or false indicating if the object can be selected</param>
    ///<returns>(Guid ResizeArray) List of identifiers of the picked objects.</returns>
    static member GetObjectsAndRemember(message:string,
                                        [<OPT;DEF(0)>]filter:int,
                                        [<OPT;DEF(true)>]group:bool,
                                        [<OPT;DEF(true)>]preselect:bool,
                                        [<OPT;DEF(false)>]select:bool,
                                        [<OPT;DEF(null:Guid seq)>]objects:Guid seq,
                                        [<OPT;DEF(1)>]minimumCount:int,
                                        [<OPT;DEF(0)>]maximumCount:int,
                                        [<OPT;DEF(true)>]printCount:bool,
                                        [<OPT;DEF(null:Input.Custom.GetObjectGeometryFilter)>]customFilter:Input.Custom.GetObjectGeometryFilter)  : Guid ResizeArray =
        let get() =
            let ids = RhinoScriptSyntax.GetObjects(message, filter, group, preselect, select, objects, minimumCount, maximumCount, printCount, customFilter)
            rememberedObjects.[message] <- ids
            ids

        match rememberedObjects.TryGetValue message with
        | false, _  ->
            get()
        | true , objectIds ->
            if objectIds.Count = 0 then
                get()
            elif objectIds.Exists (System.Predicate ( fun g -> let o =  RhinoScriptSyntax.Doc.Objects.FindId g in isNull o || o.IsDeleted )) then
                get()
            else
                if printCount then
                    RhinoScriptSyntax.Print $"GetObjectsAndRemember for '%s{message}': {RhinoScriptSyntax.ObjectDescription(objectIds)}"
                objectIds




    ///<summary>Returns the same object as in the last user interaction with the same prompt message
    /// If none found, Prompts user to pick one object and remembers it.
    /// Call rs.ClearRememberedObjects() to clear the memory.</summary>
    ///<param name="message">(string) A prompt or message, should be unique, this will be the key in dictionary to remember object</param>
    ///<param name="filter">(int) Optional, The type(s) of geometry (points, Curves, Surfaces, Meshes,...)
    ///    that can be selected. Object types can be added together to filter
    ///    several different kinds of geometry. use the RhinoScriptSyntax.Filter enum to get values, they can be joined with '+'</param>
    ///<param name="preselect">(bool) Optional, Default Value: <c>true</c>
    ///    Allow for the selection of pre-selected objects</param>
    ///<param name="select">(bool) Optional, Default Value: <c>false</c>
    ///    Select the picked objects. If False, the objects that are
    ///    picked are not selected</param>
    ///<param name="printDescr">(bool) Optional, Default Value: <c>false</c> Print object description to command window</param>
    ///<param name="customFilter">(Input.Custom.GetObjectGeometryFilter) Optional, A custom filter function</param>
    ///<returns>(Guid) a identifier of the picked object.</returns>
    static member GetObjectAndRemember( message:string,
                                        [<OPT;DEF(0)>]filter:int,
                                        [<OPT;DEF(true)>]preselect:bool,
                                        [<OPT;DEF(false)>]select:bool,
                                        [<OPT;DEF(false)>]printDescr:bool,
                                        [<OPT;DEF(null:Input.Custom.GetObjectGeometryFilter)>]customFilter:Input.Custom.GetObjectGeometryFilter)  : Guid =
        let get() =
            let id = RhinoScriptSyntax.GetObject(message, filter,  preselect, select,  customFilter, subObjects=false)
            rememberedObjects.[message] <- ResizeArray [id]
            id

        match rememberedObjects.TryGetValue message with
        | false, _  ->
            get()
        | true , objectIds ->
            if objectIds.Count <> 1 then
                get()
            elif (let o = RhinoScriptSyntax.Doc.Objects.FindId objectIds.[0] in isNull o || o.IsDeleted ) then
                get()
            else
                if printDescr then
                    RhinoScriptSyntax.Print $"GetObjectAndRemember for '%s{message}': {RhinoScriptSyntax.ObjectDescription(objectIds.[0])}"
                objectIds.[0]


    ///<summary>Clears all remembered objects form internal Dictionary that where added via  rs.GetObjectAndRemember() or rs.GetObjectsAndRemember()</summary>
    static member ClearRememberedObjects()  : unit =
        // RhinoScriptSyntax.PrintfGray "Cleared %d remembered Selection Sets" rememberedObjects.Count
        rememberedObjects.Clear()