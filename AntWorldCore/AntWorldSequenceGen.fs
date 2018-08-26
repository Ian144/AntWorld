module AntWorldEntryPoint

open Types
open NestFuncs
open AntWorldFuncs
open System



let noDirection = { dx = 0.0<distance>; dy = 0.0<distance> }
let originLoc = { x = 0.0<distance>; y = 0.0<distance> }

let randGen = System.Random()




let private NoOcclusion (aa:IRadLoc) (bb:IRadLoc)  = 
    let distBetween = LocationFuncs.CalcDistance aa.GetLoc bb.GetLoc
    let sumRadius = aa.GetRadius + bb.GetRadius
    distBetween > sumRadius


let private NoOcclusionObstacleNest (nests:Nest list ) (ob:Obstacle) = List.forall (NoOcclusion ob) nests


let private NoOcclusionFoodObstacle (foodItems:FoodItem list ) (ob:Obstacle) = List.forall (NoOcclusion ob) foodItems


let private MakeFoodItems numFoodItems (getRandLoc:Unit -> Location)  =    
        let foodItem1 = { loc = {x = 0.0<distance>; y = 0.0<distance>}; amountFood = 10000<food>} 
        Seq.unfold (fun a -> Some (a, a)) foodItem1  |> 
        Seq.take numFoodItems |>  
        Seq.map (fun fd -> {fd with loc = getRandLoc()}) |>  
        Seq.toList



let private MakeRandObstacle (getRandLoc:Unit -> Location) : Obstacle =
    let rad = LanguagePrimitives.FloatWithMeasure (randGen.NextDouble() * 50.0 + 25.0)
    {radius = rad; loc = getRandLoc() }


// make obstacles that do not cover any food items or nests
let private MakeObstacles numObstacles foodItems nests (getRandLoc:Unit -> Location) =   
        seq {1 .. numObstacles} |> 
        Seq.map (fun _ -> MakeRandObstacle getRandLoc) |>
        Seq.filter (NoOcclusionFoodObstacle foodItems) |> 
        Seq.filter (NoOcclusionObstacleNest nests) |> 
        Seq.toList


let private MakeNest (numAnts:int) getRandLoc =
    let nestLoc = getRandLoc() 
    let ant1 = {foodStored = 1<food>; loc = nestLoc; prevLocs = []; nestLoc = nestLoc; state = InNest }
    let antList = Seq.unfold (fun a -> Some (a, a)) ant1 |> Seq.take numAnts |> Seq.toList
    { Ants = antList; FoodStore = 1000000<food>; Loc = nestLoc }        



let private MakeNests (numNests:int) (numAntsPerNest:int) (getRandLoc:Unit -> Location) : Nest list =
    let antSeq = seq { for n in 1 .. numNests do
                       yield MakeNest numAntsPerNest getRandLoc} 
    Seq.toList antSeq
 



let MakeAntWorldSeq (numAntsPerNest:int)  (numNests:int) (numFoodItems:int) (numObstacles:int) (range:int)  =

    let GetRandLocationRange = fun() -> let fRange = float range
                                        let gr () = LanguagePrimitives.FloatWithMeasure<distance> (fRange * (randGen.NextDouble() * 2.0 - 1.0) )
                                        {x = gr(); y = gr()}

    let nests = MakeNests numNests numAntsPerNest GetRandLocationRange
    let foodItems = MakeFoodItems numFoodItems GetRandLocationRange
    let obstacles = MakeObstacles numObstacles foodItems nests GetRandLocationRange

    // make the '1st frame' antWorld, subsequent frames are created from the previous one
    let initialAntWorld = {  nests = nests; 
                             foodItems = foodItems;
                             trails = Trail();
                             obstacles = obstacles }

    Seq.unfold OptUpdateWorld initialAntWorld
