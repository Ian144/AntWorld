module Utilities



let FirstOrNone xs = 
    if Seq.isEmpty xs then
       None
    else
       Some (Seq.head xs)


// apply a raw float conversion function, e.g. System.Math.Floor to a float<unit_of_measure>
let UnitOfMeasureFloatApply (func: float->float) (xd:float<_>)  : float<_> = 
    let x = float xd
    let x' = func x
    LanguagePrimitives.FloatWithMeasure<_> x'



