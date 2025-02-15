[<RequireQualifiedAccess>]
module LocationFuncs

open Types
open Checked
open Utilities

let zeroDirection =
    { dx = 0.0<distance>
      dy = 0.0<distance> }

let ConstrainToStepSize (mv: MoveVec) (stepSize: float<distance>) =
    let dist = sqrt(mv.dx * mv.dx + mv.dy * mv.dy)
    let xx = stepSize / dist
    { dx = mv.dx * xx; dy = mv.dy * xx }

let CalcDistance (loc1: Location) (loc2: Location) =
    let dx = loc1.x - loc2.x
    let dy = loc1.y - loc2.y
    sqrt(dx * dx + dy * dy)


// relative to horizontal
let CalcAngle (loc1: Location) (loc2: Location) : float =
    let dx = float(loc1.x - loc2.x)
    let dy = float(loc1.y - loc2.y)
    System.Math.Atan2(dy, dx)

let private Quantise (x: float<_>) (q: float) =
    let x1 = UnitOfMeasureFloatApply System.Math.Floor (x / q)
    x1 * q


// removes the fractional part of the x and y coords of a location
// like 'snap to grid'
let QuantiseLocation (loc: Location) quantiseLevel : Location =
    let xf = Quantise loc.x quantiseLevel
    let yf = Quantise loc.y quantiseLevel
    { x = xf; y = yf }
