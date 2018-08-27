
[<RequireQualifiedAccess>]
module LocationFuncs

open Types
open Checked
open Utilities



let zeroDirection = {dx=0.0<distance>; dy=0.0<distance>}



let ConstrainToStepSize (mv:MoveVec) (stepSize:float<distance>) = 
    let dist = sqrt( mv.dx * mv.dx + mv.dy * mv.dy )
    let xx = stepSize / dist
    {dx = mv.dx*xx; dy = mv.dy*xx}


let setX oldLoc newX = { oldLoc with x = newX }
let setY oldLoc newY = { oldLoc with y = newY }


//let sqr (x:float<_>) = x*x
//let cube (x:float<_>) = x*x*x
//let silly x y = sqr x + cube y



let CalcDistance (loc1:Location) (loc2:Location) = 
    let dx = loc1.x - loc2.x
    let dy = loc1.y - loc2.y
    sqrt (dx * dx + dy * dy )


// relative to horizontal
let CalcAngle (loc1:Location) (loc2:Location) : float = 
    let dx = float(loc1.x - loc2.x)
    let dy = float(loc1.y - loc2.y)
    System.Math.Atan2 (dy, dx)


//// removes the fractional part of the x and y coords
//let FloorLocation (loc:Location) : Location = 
//      let xf = UnitOfMeasureFloatApply System.Math.Floor loc.x
//      let yf = UnitOfMeasureFloatApply System.Math.Floor loc.y
//      {x = xf; y = yf}


let private Quantise (x:float<_>) (q:float) = 
    let x1 = UnitOfMeasureFloatApply System.Math.Floor (x / q)
    x1 * q
    

// removes the fractional part of the x and y coords of a location
// like 'snap to grid'
let QuantiseLocation (loc:Location)  quantiseLevel : Location = 
      let xf = Quantise loc.x quantiseLevel
      let yf = Quantise loc.y quantiseLevel
      {x = xf; y = yf}


// calculate the move vector for 8 surrounding (above, above right, right, below right etc) locations with a given stepSize
let CalcSurroundingLocDirections stepSize = let vecX = [-1.0<distance>; 0.0<distance>; 1.0<distance> ]
                                            let vecY = vecX
                                            let tmp1 = [for xx in vecX do for yy in vecY do yield {dx = xx; dy = yy}]
                                            let tmp2 = List.truncate 4 tmp1 @ List.skip 5 tmp1  // remove the center, only want surrounding steps
                                            List.map (fun mv -> ConstrainToStepSize mv stepSize) tmp2