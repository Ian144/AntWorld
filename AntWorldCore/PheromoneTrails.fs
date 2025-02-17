module PheromoneTrails

open Types
open CollisionFuncs
open Microsoft.FSharp.Collections

let trailLocQuantisation = 4.0

let trailLocQuantisationDistance = 1.0<distance> * trailLocQuantisation


let TrailDetected (trail: Trail) loc =
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, _ = trail.TryGetValue loc2
    found

// let GetSurroundingLocationsForLoop(loc: Location) : Location list =
//     let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
//     let x = loc2.x
//     let y = loc2.y
//     let vecX = [ x - trailLocQuantisationDistance; x; x + trailLocQuantisationDistance ]
//     let vecY = [ y - trailLocQuantisationDistance; y; y + trailLocQuantisationDistance ]
//     [ for xx in vecX do
//           for yy in vecY do
//               if xx <> x || yy <> y then
//                   yield { x = xx; y = yy } ] // Avoid computing center element

// this is faster than using nested for-loops
let GetSurroundingLocations(loc: Location) : Location list =
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let x = loc2.x
    let y = loc2.y
    let d = trailLocQuantisationDistance
    [ { x = x - d; y = y - d }
      { x = x - d; y = y     }
      { x = x - d; y = y + d }
      { x = x    ; y = y - d }
      { x = x    ; y = y + d }
      { x = x + d; y = y - d }
      { x = x + d; y = y     }
      { x = x + d; y = y + d } ]


// get the nearest pheromone location not covered by an obstacle
// occasionally there is no uncovered local location, returns a 'Location option' to represent this
let GetNearestUncoveredPheromoneLocationIan (obstacles: Obstacle list) (location: Location) : Location option =
    let surLocs = GetSurroundingLocations location
    let surLocs2 = List.filter (CollisionFilter obstacles) surLocs

    let surLocs3 =
        surLocs2
        |> List.map(fun surLoc -> (surLoc, (LocationFuncs.CalcDistance surLoc location)))
        |> List.sortBy(fun (_, dist) -> 1.0 / dist)

    match surLocs3 with
    | [] -> None
    | _ -> Some(fst surLocs3.Head)

// imperative on the inside
let GetNearestUncoveredPheromoneLocation (obstacles: Obstacle list) (location: Location) : Location option =
    let surLocs = GetSurroundingLocations location
    let mutable bestDist = LanguagePrimitives.FloatWithMeasure<distance> System.Double.MaxValue
    let mutable bestLocation = None
    for candidate in surLocs do
        if CollisionFilter obstacles candidate then
            let dist = LocationFuncs.CalcDistance candidate location
            if dist < bestDist then
                bestDist <- dist
                bestLocation <- Some candidate
    bestLocation

let UpdateTrail (trail: Trail) (obs: Obstacle list) (loc: Location) : Trail =
    let optLoc2 = GetNearestUncoveredPheromoneLocation obs loc

    match optLoc2 with
    | None -> trail
    | Some loc2 ->
        let found, oldVal = trail.TryGetValue loc2

        match found with
        | false -> trail.Add(loc2, 1.0)
        | true -> trail.Add(loc2, (1.0 + oldVal))


let GetPheromoneLevel loc (trail: Trail) =
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, level = trail.TryGetValue loc2

    match found with
    | false -> 0.0
    | true -> level


let fadeFactor = 0.9985
let minLevel = 0.05

//pheromone trails fade with time if not renewed
let FadeTrailsArray(trails: Trail) : Trail =
    let xs = trails |> Map.toArray

    let ys =
        [| for loc, pheromoneLevel in xs do
               let pheromoneLevel2 = pheromoneLevel * fadeFactor
               if pheromoneLevel2 > 0.1 then
                   yield loc, pheromoneLevel2 |]

    ys |> Map.ofArray

let FadeTrailsEmpty(trails: Trail) : Trail = trails

let FadeTrailsMapFilter(trails: Trail) : Trail =
    trails |> Map.map(fun _ v -> (v * fadeFactor)) |> Map.filter(fun _ v -> v > minLevel)

let FadeTrailsFold(trails: Trail) : Trail =
    trails |> Map.fold (fun acc key value ->
                    let newValue = value * fadeFactor
                    if newValue > minLevel then Map.add key newValue acc else acc)
                    Map.empty
