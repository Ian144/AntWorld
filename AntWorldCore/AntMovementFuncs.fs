module AntMovement

open Types
open Checked
open PheromoneTrails
open CollisionFuncs


let isStuckLookback = 10
let isStuckDetectionFactor = 0.2

let UpdateLoc (ant: Ant) newLoc =
    let prevLocs2 = newLoc :: (List.truncate (isStuckLookback - 1) ant.prevLocs)
    { ant with
        prevLocs = prevLocs2
        loc = newLoc }

let IsStuck (ant: Ant) =
    if ant.prevLocs.Length = isStuckLookback then
        let last = Seq.rev ant.prevLocs |> Seq.head
        let distMoved = LocationFuncs.CalcDistance ant.loc last
        distMoved < (antStepSize * (float isStuckLookback) * isStuckDetectionFactor)
    else
        false

let private randGen = System.Random(999)

let OneInTen () = randGen.NextDouble() < 0.1


let GetRandomMoveVec () =
    let GetRandDist () =
        (randGen.NextDouble() * 2.0 * antStepSize - antStepSize)

    { dx = GetRandDist()
      dy = GetRandDist() }


// move ant at curLoc towards destination
let MoveTowards (destLoc: Location) (curLoc: Location) (stepSize: float<distance>) : Location =
    let distance = LocationFuncs.CalcDistance destLoc curLoc
    let angle = LocationFuncs.CalcAngle destLoc curLoc
    if distance >= stepSize then
        { x = curLoc.x + stepSize * cos angle
          y = curLoc.y + stepSize * sin angle }
    else
        destLoc

// see MoveTowardsRnd comment
let MoveTowardsRndImpl
    (rndMove: Unit -> bool)
    (destLoc: Location)
    (curLoc: Location)
    (stepSize: float<distance>)
    : Location =
    if rndMove () then
        let randMove = GetRandomMoveVec()
        { curLoc with
            x = curLoc.x + randMove.dx
            y = curLoc.y + randMove.dy }
    else
        MoveTowards destLoc curLoc stepSize


// as MoveTowards but with occasional random movements
// same signature as MoveTowards but with partial application
let MoveTowardsRnd = MoveTowardsRndImpl OneInTen



// precalculate the directions (MoveVecs) for 8 surrounding steps from any point (above, above right, right, below right etc)
let surroundingStepDirections =
    let vecX = [ -1.0<distance>; 0.0<distance>; 1.0<distance> ]
    let vecY = vecX
    let tmp1 =
        [| for xx in vecX do
               for yy in vecY do
                   yield { dx = xx; dy = yy } |]
    let tmp2 = List.truncate 4 tmp1 @ List.skip 5 tmp1 // remove the center, only want surrounding steps
    Seq.map (fun mv -> LocationFuncs.ConstrainToStepSize mv antStepSize) tmp2


let GetAllPossibleNextSteps (loc: Location) : Location seq =
    Seq.map (fun mv -> { x = loc.x + mv.dx; y = loc.y + mv.dy }) surroundingStepDirections

let MoveTowardsWithCollisionDetection destLoc loc stepSize obstacles =
    let loc2 = MoveTowards destLoc loc stepSize
    let collisionObstacles: Obstacle seq = obstacles |> Seq.filter (CollisionTest loc2)

    match Seq.isEmpty collisionObstacles with
    | true -> loc2 // no collision
    | false ->
        let surLocs = GetAllPossibleNextSteps loc // there has been a collision with an obstacle
        let collisionFilter loc = not (AnyCollisions obstacles loc)
        let surLocs2 = surLocs |> Seq.filter collisionFilter // get 'collision free' surrounding locations
        let surLocs3 = surLocs2 |> Seq.map (fun loc -> (loc, LocationFuncs.CalcDistance loc destLoc)) // sort potential next steps by distance from destination and pick the best
        surLocs3 |> Seq.maxBy snd |> fst

let MoveFollowingTrail (ant: Ant) (aw: AntWorld)  : Location * MoveVec =
    let surLocs =
        GetAllPossibleNextSteps ant.loc 
        |> Seq.filter (CollisionFilter aw.obstacles)
           
    // sort remaining locations in order of highest pheromone level first
    let highestPheromoneLevelLoc =
        surLocs
        |> Seq.map (fun loc -> (loc, (GetPheromoneLevel loc aw.trails)))
        |> Seq.maxBy (fun (_, pLevel) -> 1.0 / pLevel)
        |> fst

    let direction =
        { dx = highestPheromoneLevelLoc.x - ant.loc.x
          dy = highestPheromoneLevelLoc.y - ant.loc.y }

    let direction2 = LocationFuncs.ConstrainToStepSize direction antStepSize

    let loc2 =
        { x = ant.loc.x + direction2.dx
          y = ant.loc.y + direction2.dy }

    loc2, direction2




let rec MoveWithMomentumAndCollisionDetection
    (ant: Ant)
    (direction: MoveVec)
    (momentumFactor: float)
    (stepSize: float<distance>)
    (obs: Obstacle list)
    =
    let MoveWithMomentum (direction: MoveVec) (momentumFactor: float) (stepSize: float<distance>) =
        let randMove = GetRandomMoveVec()

        let mv =
            { dx = direction.dx * momentumFactor + randMove.dx
              dy = direction.dy * momentumFactor + randMove.dy }

        LocationFuncs.ConstrainToStepSize mv stepSize

    let mv2 = MoveWithMomentum direction momentumFactor stepSize

    let antLoc2 =
        { x = ant.loc.x + mv2.dx
          y = ant.loc.y + mv2.dy }

    let colObs = obs |> List.filter (CollisionTest antLoc2)

    match colObs with
    | [] -> antLoc2, mv2 // no collisions as the list of collision objects is empty
    | _ ->
        let momentumFactor2 = momentumFactor * 0.25 // todo: make the collision reduction factor a parameter
        MoveWithMomentumAndCollisionDetection ant direction momentumFactor2 stepSize obs
