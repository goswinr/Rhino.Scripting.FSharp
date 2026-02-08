namespace Rhino.Scripting.FSharp

open System
open Rhino
open Rhino.Scripting
open Rhino.Scripting.RhinoScriptingUtils

//TODO fix Pretty nuget first


/// This module shadows the Pretty module from the Pretty nuget package to include the special formatting for Rhino types.
[<AutoOpen>]
module AutoOpenPrinting =

    ///<summary>Pretty formatting for Rhino and .Net types, e.g. numbers including thousand Separator and (nested) sequences, first six items are printed out.
    /// Settings are exposed in Pretty.PrettySettings:
    /// - thousandSeparator       = '     ; set this to change the printing of floats and integers larger than 10'000
    /// - maxNestingDepth         = 3     ; set this to change how deep the content of nested seq is printed (printFull ignores this)
    /// - maxItemsPerSeq          = 6     ; set this to change how many items per seq are printed (printFull ignores this)
    /// - maxCharsInString        = 2000  ; set this to change how many characters of a string might be printed at once.</summary>
    ///<param name="x">('T) the value or object to pretty-format</param>
    ///<returns>(string) A pretty formatted string.</returns>
    let pretty (x:'T) :string =
        PrettySetup.init() // the shadowing is only done to ensure init() is called once
        // Pretty.toPrettyString x
        sprintf "%A" x

    //   /// Pretty formatting for Rhino and .Net types, e.g. numbers including thousand Separator,
    //   /// all items of sequences, including nested items, are printed out.
    //   /// Settings are exposed in Pretty.PrettySettings:
    //   /// - thousandSeparator       = '      ; set this to change the printing of floats and integers larger than 10'000
    //   /// - maxCharsInString        = 2000   ; set this to change how many characters of a string might be printed at once.
    //   let toPrettyStringFull (x:'T) :string =
    //       PrettySetup.init() // the shadowing is only done to ensure init() is called once
    //       PrettyString.toPrettyStringFull x
    //       sprintf "%A" x


    ///<summary>Print to standard out including nice formatting for Rhino Objects, numbers including thousand Separator and (nested) sequences, first six items are printed out.
    /// Only prints to Console.Out, NOT to Rhino Commandline
    /// Shows numbers smaller than State.Doc.ModelAbsoluteTolerance * 0.1 as ~0.0
    /// Settings are exposed in Pretty.PrettySettings:
    /// - thousandSeparator       = '     ; set this to change the printing of floats and integers larger than 10'000
    /// - maxNestingDepth         = 3     ; set this to change how deep the content of nested seq is printed (printFull ignores this)
    /// - maxItemsPerSeq          = 6     ; set this to change how many items per seq are printed (printFull ignores this)
    /// - maxCharsInString        = 2000  ; set this to change how many characters of a string might be printed at once.</summary>
    ///<param name="x">('T) the value or object to print</param>
    ///<returns>(unit) void, nothing.</returns>
    let print x =
        PrettySetup.init()
        pretty x |> Console.WriteLine


    //   /// Print to standard out including nice formatting for Rhino Objects, numbers including thousand Separator, all items of sequences, including nested items, are printed out.
    //   /// Only prints to Console.Out, NOT to Rhino Commandline
    //   /// Shows numbers smaller than State.Doc.ModelAbsoluteTolerance * 0.1 as ~0.0
    //   /// Settings are exposed in Pretty.PrettySettings:
    //   /// - thousandSeparator       = '      ; set this to change the printing of floats and integers larger than 10'000
    //   /// - maxCharsInString        = 2000   ; set this to change how many characters of a string might be printed at once.
    //   let printFull x =
    //       PrettySetup.init()
    //       printFull x



    type RhinoScriptSyntax with


        ///<summary>
        /// Pretty formatting for numbers including thousand Separator and (nested) sequences, first five items are printed out.
        /// Prints to Console.Out and to Rhino Commandline
        /// Shows numbers smaller than State.Doc.ModelAbsoluteTolerance * 0.1 as 0.0
        /// Settings are exposed in Pretty.PrettySettings:
        /// maxDepth          = 3     : how deep the content of nested seq is printed
        /// maxVertItems      = 6     : amount of lines printed.
        /// maxHorChars       = 120   : maximum amount of characters per line.
        /// maxCharsInString  = 2000  : after this the characters of a string are trimmed off.
        /// The function rs.PrintFull does not do this trimming.
        /// </summary>
        ///<param name="x">('T) the value or object to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member Print (x:'T) : unit =
            PrettySetup.init()
            let t = pretty(x)
            RhinoApp.WriteLine t
            Console.WriteLine t
            RhinoApp.Wait() // no switch to UI Thread needed !

        // ///<summary>
        // /// Pretty formatting for numbers including thousand Separator, all items of sequences, including nested items, are printed out.
        // /// Prints to Console.Out and to Rhino Commandline
        // /// Shows numbers smaller than State.Doc.ModelAbsoluteTolerance * 0.1 as 0.0</summary>
        // ///<param name="x">('T) the value or object to print</param>
        // ///<returns>(unit) void, nothing.</returns>
        // static member PrintFull (x:'T) : unit =
        //     PrettySetup.init()
        //     toPrettyStringFull(x)
        //     |>! RhinoApp.WriteLine
        //     |>  Console.WriteLine
        //     RhinoApp.Wait() // no switch to UI Thread needed !

        ///<summary>Prints in Red if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintRed msg = RhinoSync.PrintColor 220 0 0 msg

        ///<summary>Prints in Red if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnRed msg = RhinoSync.PrintnColor 220 0 0 msg

        ///<summary>Prints in Green if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintGreen msg = RhinoSync.PrintColor 0 190 0 msg

        ///<summary>Prints in Green if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnGreen msg = RhinoSync.PrintnColor 0 190 0 msg

        ///<summary>Prints in Light Blue if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintLightBlue msg = RhinoSync.PrintColor 170 210 230 msg

        ///<summary>Prints in Light Blue if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnLightBlue msg = RhinoSync.PrintnColor 170 210 230 msg

        ///<summary>Prints in Blue if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintBlue msg = RhinoSync.PrintColor 0 0 220 msg

        ///<summary>Prints in Blue if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnBlue msg =  RhinoSync.PrintnColor 0 0 220 msg

        ///<summary>Prints in Gray if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintGray msg = RhinoSync.PrintColor 160 160 160 msg

        ///<summary>Prints in Gray if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="msg">(string) The string to print</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnGray msg = RhinoSync.PrintnColor 160 160 160 msg

        ///<summary>Prints in custom color if used from Fesh F# Scripting Editor. Does not add a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="red">(int) Red Value between 0 and 255 </param>
        ///<param name="green">(int) Green value between 0 and 255 </param>
        ///<param name="blue">(int) Blue value between 0 and 255 </param>
        ///<param name="msg">The string</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintColor (red:int) (green:int) (blue:int) msg  =
            RhinoSync.PrintColor red green blue msg

        ///<summary>Prints in custom color if used from Fesh F# Scripting Editor. Adds a new line at end.
        /// Prints to Console.Out and to Rhino Commandline.</summary>
        ///<param name="red">(int) Red Value between 0 and 255 </param>
        ///<param name="green">(int) Green value between 0 and 255 </param>
        ///<param name="blue">(int) Blue value between 0 and 255 </param>
        ///<param name="msg">The format string</param>
        ///<returns>(unit) void, nothing.</returns>
        static member PrintnColor (red:int) (green:int) (blue:int) msg  =
            RhinoSync.PrintnColor red green blue msg




