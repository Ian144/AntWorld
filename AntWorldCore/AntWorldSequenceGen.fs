module AntWorldEntryPoint

open Types
open AntWorldFuncs



let noDirection =
    { dx = 0.0<distance>
      dy = 0.0<distance> }

let originLoc = { x = 0.0<distance>; y = 0.0<distance> }

let randGen = System.Random(999)

let private NoOcclusion (aa: IRadLoc) (bb: IRadLoc) =
    let distBetween = LocationFuncs.CalcDistance aa.GetLoc bb.GetLoc
    let sumRadius = aa.GetRadius + bb.GetRadius
    distBetween > sumRadius


let private NoOcclusionObstacleNest (nests: Nest list) (ob: Obstacle) = List.forall (NoOcclusion ob) nests


let private NoOcclusionFoodObstacle (foodItems: FoodItem list) (ob: Obstacle) = List.forall (NoOcclusion ob) foodItems



let private MakeFoodItemsLazy (getRandLoc: Unit -> Location) =
    seq {
        while true do
            yield
                { loc = getRandLoc ()
                  amountFood = 10000<food> }
    }


let private MakeFoodItems numFoodItems (getRandLoc: Unit -> Location) =
    [ for n in 1..numFoodItems do
          yield
              { loc = getRandLoc ()
                amountFood = 10000<food> } ]


let private MakeRandObstacle (getRandLoc: Unit -> Location) : Obstacle =
    let rad = LanguagePrimitives.FloatWithMeasure(randGen.NextDouble() * 50.0 + 25.0)
    { radius = rad; loc = getRandLoc () }


// make obstacles that do not cover any food items or nests
let private MakeObstacles numObstacles foodItems nests (getRandLoc: Unit -> Location) =
    [ 1..numObstacles ]
    |> List.map (fun _ -> MakeRandObstacle getRandLoc)
    |> List.filter (NoOcclusionFoodObstacle foodItems)
    |> List.filter (NoOcclusionObstacleNest nests)


let private MakeNest (numAnts: int) getRandLoc =
    let nestLoc = getRandLoc ()

    let ant1 =
        { foodStored = 1<food>
          loc = nestLoc
          prevLocs = []
          nestLoc = nestLoc
          state = InNest }

    let antList =
        Seq.unfold (fun a -> Some(a, a)) ant1 |> Seq.take numAnts |> Seq.toList

    { Ants = antList
      FoodStore = 1000000<food>
      Loc = nestLoc }


let private MakeNests (numNests: int) (numAntsPerNest: int) (getRandLoc: Unit -> Location) : Nest list =
    [ for n in 1..numNests do
          yield MakeNest numAntsPerNest getRandLoc ]


let MakeAntWorldSeq (numAntsPerNest: int) (numNests: int) (numFoodItems: int) (numObstacles: int) (range: int) (fadeTrailsOption) =
    let GetRandLocationRange =
        fun () ->
            let fRange = float range

            let gr () =
                LanguagePrimitives.FloatWithMeasure<distance>(fRange * (randGen.NextDouble() * 2.0 - 1.0))

            { x = gr (); y = gr () }
            
    let fadeTrails = match fadeTrailsOption with
                        | 0 -> PheromoneTrails.FadeTrailsFold
                        | 1 -> PheromoneTrails.FadeTrailsArray
                        | 2 -> PheromoneTrails.FadeTrailsMapFilter
                        | _ -> PheromoneTrails.FadeTrailsFold
    
    let nests = MakeNests numNests numAntsPerNest GetRandLocationRange
    let foodItems = MakeFoodItems numFoodItems GetRandLocationRange
    let obstacles = MakeObstacles numObstacles foodItems nests GetRandLocationRange

    // make the '1st frame' antWorld, subsequent frames are created from the previous one
    // considering local immutability to be safe
    let mutable initialAntWorld =
        { nests = nests
          foodItems = foodItems
          trails = Map.empty<Location, float>
          obstacles = obstacles }

    seq {
        while true do
            yield initialAntWorld
            initialAntWorld <- UpdateWorld initialAntWorld fadeTrails 
    }
