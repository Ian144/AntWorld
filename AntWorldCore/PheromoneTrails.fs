module PheromoneTrails

open System.Collections.Generic
open System.Linq


open Types
open Utilities
open CollisionFuncs

open Microsoft.FSharp.Collections



let trailLocQuantisation = 4.0



let TrailDetected (trail:Trail) loc = 
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, _ = trail.TryGetValue loc2
    found



//let moveVecToSurroundingPherormonLocations = CalcSurroundingLocDirections (trailLocQuantisation * 1.0<distance>)
//
//let GetSurroundingLocations (loc:Location) : Location list = 
//    let loc2 = QuantiseLocation loc trailLocQuantisation
//    moveVecToSurroundingPherormonLocations |> List.map  (fun mv -> {x = loc2.x + mv.dx; y = loc2.y + mv.dy})



let GetSurroundingLocations (loc:Location) : Location list = 
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation 
    let x = loc2.x
    let y = loc2.y
    let vecX = [x - 1.0<distance> * trailLocQuantisation; x; x + 1.0<distance> * trailLocQuantisation]
    let vecY = [y - 1.0<distance> * trailLocQuantisation; y; y + 1.0<distance> * trailLocQuantisation]
    let tmp = [for xx in vecX do for yy in vecY do yield {x = xx;y = yy}]
    List.filter (fun ll -> ll <> loc2) tmp


// i.e get the nearest pheromon location not covered by an obstacle
// occasionally there is no uncovered local location, returns a 'Location option' to represent this
let GetNearestUncoveredPheromoneLocation (obtacles: Obstacle list) (loc:Location) : Location option = 
    let surLocs = GetSurroundingLocations loc 
    let surLocs2 = List.filter (fun lc -> CollisionFilter obtacles lc) surLocs
    let surLocs3 = surLocs2 |> List.map (fun surLoc -> (surLoc, (LocationFuncs.CalcDistance surLoc loc) )) |> List.sortBy (fun (_,dist) -> 1.0/dist) 
    match surLocs3 with
    | [] -> None
    | _  -> Some (fst surLocs3.Head)




// Trail is a mutable hashmap for performance reasons, 
let UpdateTrail (trail:Trail) (obs: Obstacle list) (loc:Location) : Trail = 
    let optLoc2 = GetNearestUncoveredPheromoneLocation obs loc 
    match optLoc2 with
    | None -> trail
    | Some loc2 ->  let found, oldVal = trail.TryGetValue loc2
                    match found with
                    | false -> trail.[loc2] <- 1.0  
                    | true -> trail.[loc2] <- 1.0 + oldVal  
                    trail


let GetPheromoneLevel loc (trail:Trail) = 
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, level = trail.TryGetValue loc2
    match found with 
    | false -> 0.0
    | true -> level


// pheremone trails fade with time if not renewed
let FadePheremone level = 
    let fadeFactor = 0.999 //todo: pass fadeFactor in as a parameter
    let level2 = level * fadeFactor
    if level2 > 0.1 then //todo: pass the 0.1 in as a parameter
        level2
    else
        0.0


let FadeTrails (trails:Trail) : Trail  = 
    let trails2 = Trail() // cant modify a map in a foreach loop, even if its just the value being modified,
    for (kvp:KeyValuePair<Location,float>) in trails do 
        let v2 = FadePheremone kvp.Value
        if v2 > 0.0 then
            trails2.[kvp.Key] <- v2
    trails


// make this cache coherent, using an array of value types
// will false sharing be an issue?
let FadeTrailsP (trails:Trail) : Trail = 
    let xx = trails.Select (fun kvp -> kvp.Key, kvp.Value) 
    let yy = xx.ToArray()


    //xx |> Array.Parallel.iter (fun kvp -> printf "%A" kvp)

    let trails2 = Trail() // cant modify a map in a foreach loop, even if its just the value being modified,
    for (kvp:KeyValuePair<Location,float>) in trails do 
        let v2 = FadePheremone kvp.Value
        if v2 > 0.0 then
            trails2.[kvp.Key] <- v2
    trails