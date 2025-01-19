module PheromoneTrails



open Types
open CollisionFuncs

open Microsoft.FSharp.Collections



let trailLocQuantisation = 4.0


let TrailDetected (trail: Trail) loc =
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let found, _ = trail.TryGetValue loc2
    found


let GetSurroundingLocations (loc: Location) : Location list =
    let loc2 = LocationFuncs.QuantiseLocation loc trailLocQuantisation
    let x = loc2.x
    let y = loc2.y

    let vecX =
        [ x - 1.0<distance> * trailLocQuantisation
          x
          x + 1.0<distance> * trailLocQuantisation ]

    let vecY =
        [ y - 1.0<distance> * trailLocQuantisation
          y
          y + 1.0<distance> * trailLocQuantisation ]

    let tmp =
        [ for xx in vecX do
              for yy in vecY do
                  yield { x = xx; y = yy } ]

    List.filter (fun ll -> ll <> loc2) tmp


// i.e get the nearest pheromone location not covered by an obstacle
// occasionally there is no uncovered local location, returns a 'Location option' to represent this
let GetNearestUncoveredPheromoneLocationIan (obstacles: Obstacle list) (location: Location) : Location option =
    let surLocs = GetSurroundingLocations location
    let surLocs2 = List.filter (fun lc -> CollisionFilter obstacles lc) surLocs

    let surLocs3 =
        surLocs2
        |> List.map (fun surLoc -> (surLoc, (LocationFuncs.CalcDistance surLoc location)))
        |> List.sortBy (fun (_, dist) -> 1.0 / dist)

    match surLocs3 with
    | [] -> None
    | _ -> Some(fst surLocs3.Head)

// imperative on the inside
let GetNearestUncoveredPheromoneLocation (obstacles: Obstacle list) (location: Location) : Location option =
    let surLocs = GetSurroundingLocations location
    // Use a 'max' distance as a float<distance> for comparisons.
    // One way is LanguagePrimitives.FloatWithMeasure to embed System.Double.MaxValue.
    let mutable bestDist =
        LanguagePrimitives.FloatWithMeasure<distance> System.Double.MaxValue

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


// A deliberately slow for-loop to practice CPU profiling
let DeliberatelySlowLoop () =
    let mutable result = 0.0

    for i in 1 .. (System.Int32.MaxValue / 100) do
        let x = float i ** 0.5 // Expensive square root
        let y = sin x * cos x // Trigonometric operations
        result <- result + y // Accumulate the result

    result

//pheromone trails fade with time if not renewed
let FadeTrails (trails: Trail) : Trail =
    //let _ = DeliberatelySlowLoop()
    //System.Threading.Thread.Sleep(100);
    let xs = trails |> Map.toArray

    let ys =
        [| for loc, pheromoneLevel in xs do
               let pheromoneLevel2 = pheromoneLevel * 0.998

               if pheromoneLevel2 > 0.1 then
                   yield loc, pheromoneLevel2 |]

    ys |> Map.ofArray

let FadeTrailsEmpty (trails: Trail) : Trail = trails

let FadeTrailsMap (trails: Trail) : Trail =
    trails |> Map.map (fun _ v -> (v * 0.998)) |> Map.filter (fun _ v -> v > 0.1)

let FadeTrailsFold (trails: Trail) : Trail =
    trails
    |> Map.fold
        (fun acc key value ->
            let newValue = value * 0.998
            if newValue > 0.1 then Map.add key newValue acc else acc)
        Map.empty
