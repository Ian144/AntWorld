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




let UpdateTrail (trail:Trail) (obs: Obstacle list) (loc:Location) : Trail = 
    let optLoc2 = GetNearestUncoveredPheromoneLocation obs loc 
    match optLoc2 with
    | None -> trail
    | Some loc2 ->  let found, oldVal = trail.TryGetValue loc2
                    match found with
                    | false -> trail.Add (loc2, 1.0 )
                    | true  -> trail.Add (loc2, (1.0 + oldVal))


let GetPheromoneLevel loc (trail:Trail) = 
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, level = trail.TryGetValue loc2
    match found with 
    | false -> 0.0
    | true -> level


// pheremone trails fade with time if not renewed
let FadePheremone level = 
    let fadeFactor = 0.95
    let level2 = level * fadeFactor
    if level2 > 0.1 then
        level2
    else
        0.0


let FadeTrails (trails:Trail) : Trail  = 
    let mutable trails2 = trails // considering mutability that does not escape a function to be ok
    for (kvp:KeyValuePair<Location,float>) in trails do 
        let v2 = FadePheremone kvp.Value
        if v2 > 0.0 then
            trails2 <- trails2.Add (kvp.Key, v2)
    trails2
