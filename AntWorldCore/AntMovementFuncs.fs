module AntMovement

open Types
open FSharpx.State
open Checked
open Utilities
open PheromoneTrails
open CollisionFuncs


let isStuckLookback = 10
let isStuckDetectionFactor = 0.2




let UpdateLoc (ant:Ant) newLoc = 
  
    let prevLocs2 = newLoc :: (List.truncate (isStuckLookback - 1) ant.prevLocs)
    {ant with prevLocs = prevLocs2; loc = newLoc}



let IsStuck (ant:Ant) = 
    if ant.prevLocs.Length = isStuckLookback then
        let last = List.rev ant.prevLocs |> List.head
        let distMoved = LocationFuncs.CalcDistance ant.loc last
        distMoved < (antStepSize *  (float isStuckLookback) * isStuckDetectionFactor)
    else
        false




let private randGen = System.Random()

let OneInTen() = randGen.NextDouble() < 0.1

let GetRandomMoveVec () =
    let GetRandDist () = (randGen.NextDouble() * 2.0 * antStepSize - antStepSize)
    {dx = GetRandDist (); dy = GetRandDist ()}
    


// move ant at curLoc towards destination
let MoveTowards (destLoc:Location) (curLoc:Location) (stepSize:float<distance>) : Location = 
    let distance = LocationFuncs.CalcDistance destLoc curLoc
    let angle = LocationFuncs.CalcAngle destLoc curLoc
    if distance >= stepSize then
        let distance2 = distance - stepSize
        { x = curLoc.x + stepSize * cos(angle) ; y = curLoc.y + stepSize * sin(angle) }
    else
        destLoc



// see MoveTowardsRnd comment
let MoveTowardsRndImpl (rndMove:Unit -> bool) (destLoc:Location) (curLoc:Location) (stepSize:float<distance>) : Location = 
    if rndMove() then
        let randMove = GetRandomMoveVec ()
        {curLoc with x = curLoc.x + randMove.dx; y = curLoc.y + randMove.dy}
    else
        MoveTowards destLoc curLoc stepSize


// as MoveTowards but with occasional random movements
// same signature as MoveTowards but with partial application
let MoveTowardsRnd =  MoveTowardsRndImpl OneInTen



// precalculate the directions (MoveVecs) for 8 surrounding steps from any point (above, above right, right, below right etc)
let surroundingStepDirections = let vecX = [-1.0<distance>; 0.0<distance>; 1.0<distance> ]
                                let vecY = vecX
                                let tmp1 = [for xx in vecX do for yy in vecY do yield {dx = xx; dy = yy}]
                                let tmp2 = List.truncate 4 tmp1 @ List.skip 5 tmp1  // remove the center, only want surrounding steps
                                List.map (fun mv -> LocationFuncs.ConstrainToStepSize mv antStepSize) tmp2 


let GetAllPossibleNextSteps (loc:Location) stepSize: Location list = 
    List.map (fun mv -> {x = loc.x + mv.dx; y = loc.y + mv.dy }) surroundingStepDirections
    


let MoveTowardsWithCollisionDetection destLoc loc stepSize obstacles = 
    let loc2 = MoveTowards destLoc loc stepSize
    let colObs = obstacles |> List.filter (CollisionTest loc2)
    match colObs with 
    | [] -> loc2 // no collision
    | _ ->  let surLocs = GetAllPossibleNextSteps loc stepSize// there has been a collision with an obstacle
            let collisionFilter loc = not (AnyCollisions obstacles loc)
            let surLocs2 = surLocs |> List.filter collisionFilter  // get 'collision free' surrounding locations
            let surLocs3 = surLocs2 |> List.map (fun loc -> (loc, LocationFuncs.CalcDistance loc destLoc)) // sort potential next steps by distance from destination and pick the best
            let surLocs4 = surLocs3 |> List.sortBy (fun (loc, dist) -> dist)
            fst surLocs4.Head



let MoveFollowingTrail (ant:Ant) (aw:AntWorld) stepSize: Location*MoveVec =
    let surLocs = GetAllPossibleNextSteps ant.loc stepSize |> List.filter (CollisionFilter aw.obstacles)
    // filter out surrounding locations closest to the nest unless current location is the nest
    let surLocDistances = surLocs |> List.map (fun loc -> (loc, (LocationFuncs.CalcDistance loc ant.nestLoc) )) |> List.sortBy (fun (_,dist) -> 1.0/dist) 
    let surLocDistances2 = if ant.loc <> ant.nestLoc then 
                                surLocDistances |> List.truncate 4 |> Seq.toList |> List.map (fun (loc,_) -> loc) 
                            else
                                surLocDistances |> List.map (fun (loc,_) -> loc)  
    // sort remaining locations in order of highest pheromone level first
    let surLocPhrmnLevels = surLocDistances2 |> List.map (fun loc -> (loc, (GetPheromoneLevel loc aw.trails))) |> List.sortBy (fun (_,pLevel) -> 1.0/pLevel)
    let highestPhrmnLevelLoc = fst(List.head surLocPhrmnLevels) // found the surrounding location with the highest level
    let direction = {dx = highestPhrmnLevelLoc.x - ant.loc.x; dy = highestPhrmnLevelLoc.y - ant.loc.y }
    let direction2 = LocationFuncs.ConstrainToStepSize direction antStepSize
    let loc2 = {x = ant.loc.x + direction2.dx; y = ant.loc.y + direction2.dy} 
    loc2, direction2




let rec MoveWithMomentumAndCollisionDetection (ant:Ant) (direction:MoveVec) (momentumFactor:float) (stepSize:float<distance>) (obs:Obstacle list) = 
        let MoveWithMomentum (direction:MoveVec) (momentumFactor:float) (stepSize:float<distance>) = 
            let randMove = GetRandomMoveVec ()
            let mv = {dx = direction.dx * momentumFactor + randMove.dx; dy = direction.dy * momentumFactor  + randMove.dy}
            LocationFuncs.ConstrainToStepSize mv stepSize
        let mv2 = MoveWithMomentum direction momentumFactor stepSize
        let antLoc2 = {x = ant.loc.x + mv2.dx; y = ant.loc.y + mv2.dy} 
        let colObs = obs |> List.filter (CollisionTest antLoc2)
        match colObs with 
        | [] -> antLoc2, mv2 // no collisions as the list of collision objects is empty
        | _ -> let momentumFactor2 = momentumFactor * 0.25 // todo: make the collision reduction factor a parameter
               MoveWithMomentumAndCollisionDetection ant direction momentumFactor2 stepSize obs 


















