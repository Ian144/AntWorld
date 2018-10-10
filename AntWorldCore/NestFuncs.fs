module NestFuncs

open Types
open AntMovement
open PheromoneTrails


let AntMaxFoodCapacity = 4000<food>


// AntMaxFoodSearchThreshold is the amount of food an ant in the nest must have before leaving to search for food
let AntFoodLevelSearchStart = AntMaxFoodCapacity / 2


// "Food store is low, give up search and return for more food" threshold
let AntFoodSearchEndThreshold =  10<food>


let FeedResidentAnt (ant:Ant) (nest:Nest) = 
    let antFoodRoom = AntMaxFoodCapacity - ant.foodStored
    let availableFood = nest.FoodStore
    let amountToEat = match antFoodRoom with 
                      | afr when afr <= availableFood -> antFoodRoom
                      | _ -> availableFood
    let ant2 = {ant with foodStored = amountToEat + ant.foodStored ; state = AntState.SearchingForFood {dx = 0.0<distance>; dy = 0.0<distance>}} 
    let nest2 = {nest with FoodStore = nest.FoodStore - amountToEat}
    (ant2, nest2)


let UpdateAntDetectedFood (ant:Ant) (foodItem:FoodItem) (antWorld:AntWorld) (stepSize:float<distance>) = 
    let antLoc2 = MoveTowards foodItem.loc ant.loc stepSize 
    if FoodFuncs.HasReachedFood ant.loc foodItem then
        let (ant2, foodItem2) = FoodFuncs.TakeFood ant foodItem 
        let foodItems2 = FoodFuncs.UpdateFoodItemList antWorld.foodItems foodItem2
        let antWorld2 = {antWorld with foodItems = foodItems2} 
        (ant2, antWorld2 )
    else
        let ant2 = {ant with loc = antLoc2}
        (ant2, antWorld)


let numGetUnstuckAttempts = 64 // todo: make the number of 'try to get unstuck' steps configurable


let UpdateAntReturnToNestHungary (ant:Ant) (antWorld:AntWorld) (stepSize:float<distance>) : (Ant*AntWorld) = 
    if IsStuck ant then
        let ant2 = {ant with state = GettingUnStuck(ant.state, numGetUnstuckAttempts, LocationFuncs.zeroDirection) } 
        (ant2, antWorld)
    else
        let loc2 = MoveTowardsWithCollisionDetection ant.nestLoc ant.loc stepSize antWorld.obstacles
        let ant2 = UpdateLoc ant loc2
        if loc2 = ant.nestLoc then
            let ant3 = {ant2 with state = AntState.InNest}
            (ant3, antWorld)
        else
            let optFd = FoodFuncs.FoodDetected ant.loc antWorld.foodItems
            let ant3 =  match optFd with
                        | Some foodItem -> {ant2 with state = DetectedFood foodItem }
                        | None -> ant2
            (ant3, antWorld)



let UpdateAntReturnToNestWithFood (ant:Ant) (foodCarried:int<food>) (nest:Nest) (antWorld:AntWorld) (stepSize:float<distance>) : Ant*Nest*AntWorld = 
    if IsStuck ant  then
        let ant2 = {ant with state = GettingUnStuck(ant.state, numGetUnstuckAttempts, LocationFuncs.zeroDirection) }
        (ant2, nest, antWorld)
    else
        let loc2 = MoveTowardsWithCollisionDetection ant.nestLoc ant.loc stepSize antWorld.obstacles
        if loc2 = ant.nestLoc then  // if arrived at the nest
            let ant2 = {(UpdateLoc ant loc2) with state = AntState.InNest}
            ant2, {nest with FoodStore = nest.FoodStore + foodCarried}, antWorld
        else
            let trails2 = UpdateTrail antWorld.trails antWorld.obstacles loc2
            let ant2 = UpdateLoc ant loc2
            let antWorld2 = {antWorld with trails = trails2}
            ant2, nest, antWorld2 





// if the ant finds food state becomes DetectedFood
// if the ant walks off the end of the trail state becomes UpdateAntSearchingForFood
// trails are collision free
let UpdateAntFollowingTrail (ant:Ant) (antWorld:AntWorld) (stepSize:float<distance>) : Ant = 
    let loc2, direction = MoveFollowingTrail ant antWorld stepSize
    let optFd = FoodFuncs.FoodDetected ant.loc antWorld.foodItems
    match optFd with 
    | Some foodItem -> {(UpdateLoc ant loc2) with state = DetectedFood foodItem}
    | None ->   let found = TrailDetected antWorld.trails loc2
                match found with
                | true  -> {(UpdateLoc ant loc2) with state = FollowingTrail direction}
                | false -> {(UpdateLoc ant loc2) with state = SearchingForFood direction} // in case the ant as walked off the end of a fading trail


let momentumFactor = 8.0


// random walk with momentum for N frames (unStickCount), then revert to previous state
let UpdateAntGettingUnStuck (ant:Ant) direction oldState unStickCount (antWorld:AntWorld) (stepSize:float<distance>) = 
    if unStickCount > 0 then
        let loc2, dir2 = MoveWithMomentumAndCollisionDetection ant direction momentumFactor stepSize antWorld.obstacles
        let ant2 = {ant with loc = loc2; state = GettingUnStuck (oldState, unStickCount - 1 , dir2) }
        // continue to drop pheromone if inner state is returning with food
        let antWorld2 = match oldState with
                        | ReturnToNestHungary | GettingUnStuck _ | SearchingForFood _ | FollowingTrail _ | InNest | DetectedFood _  -> antWorld
                        | ReturnToNestWithFood _ -> let trails2 = UpdateTrail antWorld.trails antWorld.obstacles loc2
                                                    {antWorld with trails = trails2}
        ant2, antWorld2
    else
        {ant with state = oldState}, antWorld




// Ant searching trajectories modeled using brownian motion with momentum
// i.e. ants take the next step following a brownian motion weighted in the direction that they have been following.
// ant might find food or a trail
let UpdateAntSearchingForFood (ant:Ant) (direction:MoveVec) (antWorld:AntWorld) (stepSize:float<distance>) : Ant = 
        let loc2, direction = MoveWithMomentumAndCollisionDetection ant direction momentumFactor stepSize antWorld.obstacles
        let optFd = FoodFuncs.FoodDetected loc2 antWorld.foodItems
        match optFd with  
        | Some foodItem -> {(UpdateLoc ant loc2) with state = DetectedFood foodItem}
        | None ->   let found = TrailDetected antWorld.trails ant.loc
                    match found with
                    | true  -> {(UpdateLoc ant loc2) with state = FollowingTrail direction}
                    | false -> {(UpdateLoc ant loc2) with state = SearchingForFood direction} // same state, different direction



let FuncX (ant:Ant) (nest:Nest) (antWorld:AntWorld) = 
        match ant.state with 
        | InNest -> let ant2, nest2 = FeedResidentAnt ant nest
                    (ant2, nest2, antWorld)
        | SearchingForFood dir -> let ant2 = UpdateAntSearchingForFood ant dir antWorld antStepSize 
                                  (ant2, nest, antWorld)
        | DetectedFood foodItem -> let ant2, antWorld2 = UpdateAntDetectedFood ant foodItem antWorld antStepSize 
                                   (ant2, nest, antWorld2)
        | FollowingTrail dir -> let ant2 = UpdateAntFollowingTrail ant antWorld antStepSize 
                                (ant2, nest, antWorld)
        | ReturnToNestHungary -> let ant2, antWorld2 = UpdateAntReturnToNestHungary ant antWorld antStepSize
                                 (ant2, nest, antWorld2)
        | ReturnToNestWithFood foodCarried -> let ant2, nest2, antWorld2 = UpdateAntReturnToNestWithFood ant foodCarried nest antWorld antStepSize
                                              (ant2, nest2, antWorld2)
        | GettingUnStuck (oldState, unStuckCount, dir) -> let ant2, antWorld2 = UpdateAntGettingUnStuck ant dir oldState unStuckCount antWorld antStepSize
                                                          (ant2, nest, antWorld2)


let rec MonadicUpdateAnts (ants : Ant list) (nest : Nest) (antWorld:AntWorld) = 
    //let mutable ants = ants
    //let mutable nest = nest
    //let mutable antWorld = antWorld
    if ants.IsEmpty then  
        ants, nest, antWorld
    else

        // how to replicate state monad updating with a while loop 
        // - DOES THIS REQUIRE YIELD-RETURN, WHICH BASICALLY MAKES IT A FOLD, OR MAP
        // - or does it require adding to an mutable updateable list
        // how to replicate state monad update with a fold - this feels more natural
        //  THE STATE MUST include the nest and the world, but maybe the fold can be over the ant list only
        //   
        // ZIP ZIP ZIP??

        //let xs = List.fold


        //let ant = ants.Head
        //let (ant2, nest2, antWorld2) =  match ant.state with 
        //                                | InNest -> let ant2, nest2 = FeedResidentAnt ant nest
        //                                            (ant2, nest2, antWorld)
        //                                | SearchingForFood dir -> let ant2 = UpdateAntSearchingForFood ant dir antWorld antStepSize 
        //                                                          (ant2, nest, antWorld)
        //                                | DetectedFood foodItem -> let ant2, antWorld2 = UpdateAntDetectedFood ant foodItem antWorld antStepSize 
        //                                                           (ant2, nest, antWorld2)
        //                                | FollowingTrail dir -> let ant2 = UpdateAntFollowingTrail ant antWorld antStepSize 
        //                                                        (ant2, nest, antWorld)
        //                                | ReturnToNestHungary -> let ant2, antWorld2 = UpdateAntReturnToNestHungary ant antWorld antStepSize
        //                                                         (ant2, nest, antWorld2)
        //                                | ReturnToNestWithFood foodCarried -> let ant2, nest2, antWorld2 = UpdateAntReturnToNestWithFood ant foodCarried nest antWorld antStepSize
        //                                                                      (ant2, nest2, antWorld2)
        //                                | GettingUnStuck (oldState, unStuckCount, dir) -> let ant2, antWorld2 = UpdateAntGettingUnStuck ant dir oldState unStuckCount antWorld antStepSize
        //                                                                                  (ant2, nest, antWorld2)
                                                                                                        
        (ants, nest, antWorld)



let AntsUseUpFood (ants:Ant List) : Ant List = 
    [   for ant in ants do
        let ant' = {ant with foodStored = ant.foodStored - 1<food>} 
        if (ant'.foodStored < AntFoodSearchEndThreshold) then
            let ant'' = match ant'.state with 
                        | InNest | ReturnToNestHungary | ReturnToNestWithFood _ | GettingUnStuck _ -> ant'
                        | SearchingForFood _ | DetectedFood _ | FollowingTrail _ -> {ant' with state = ReturnToNestHungary}
            yield ant''
        else
            yield ant']
    
       

let UpdateNest2 (nest:Nest) (antWorld:AntWorld) : (Nest*AntWorld) = 
    let ants = AntsUseUpFood nest.Ants // all ants burn up 1 food unit per iteration
    let ants2, nest2, antWorld2 = MonadicUpdateAnts ants nest antWorld
    let nest3 = { Ants = ants2;  // todo nest 3 here, was old antworld correctly updating  the food store
                  FoodStore = nest2.FoodStore; 
                  Loc = nest.Loc } 
    ( nest3, antWorld2)



