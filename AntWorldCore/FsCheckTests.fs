module FsCheckTests

open Types
open AntMovement
open FSharpx.State
open Checked
open FsCheck
open System
open Microsoft.FSharp.Math

open Prop


let fpEquals (f1:float) (f2:float) = 
    let diff = abs(f1 - f2)
    diff < 0.00001


let maxDist =  System.Double.MaxValue / 10000000.0
let minDist =  10.0 // so distance to the nest is much greater than step size



let NumFilter (x:float) : bool = 
    match x with
    | x when x = 0.0 -> false    
    | x when (abs x) > maxDist -> false    
    | x when (abs x) < minDist -> false    
    | x when Double.IsNaN x -> false
    | x when Double.IsInfinity x -> false
    | _ -> true


let LocFilter (loc:Location) : bool = 
    let x = (float loc.x)
    let y = (float loc.y)
    (NumFilter x) && (NumFilter y)


//let private nestDistLess (loc:Location) = 
//        let destLoc = {x = 0.0<distance>; y = 0.0<distance>}
//        let dist = CalcDistance loc destLoc
//        let loc' = CalcNextReturnToNestLocationImpl loc destLoc 1.0<distance>
//        let dist' = CalcDistance loc' destLoc
//        dist > dist'


//let private nestAngleSame (loc:Location) = 
//        let destLoc = {x = 0.0<distance>; y = 0.0<distance>}
//        let angle = CalcAngle loc destLoc
//        let loc' = CalcNextReturnToNestLocationImpl loc destLoc 1.0<distance>
//        let angle' = CalcAngle loc' destLoc
//        fpEquals angle angle'
//
//
//let nestDistLessFilt (loc:Location) = LocFilter loc ==> nestDistLess loc
//let nestAngleSameFilt (loc:Location) = LocFilter loc ==> nestAngleSame loc
//
//
//let cfg = { Config.Default with MaxTest = 1000
//                                MaxFail = 10}
//
//
//Check.Quick( "nestDistLess", nestDistLessFilt )
//Check.Quick( "nestAngleSame", nestAngleSameFilt )
