module AntWorldFuncs


open Types
open NestFuncs
open PheromoneTrails



// my version
let NestsUpdaterIan (nests: Nest list) (awIn: AntWorld) : (Nest list * AntWorld) =
    let mutable aw = awIn
    let nests2 =
        [ for nest in nests do
              let nest', awTmp = UpdateNest2 nest aw
              aw <- awTmp
              yield nest' ]
    (nests2, aw)

// impl by riders AI
let NestsUpdater (nests: Nest list) (awIn: AntWorld) : (Nest list * AntWorld) =
    let arr = Array.ofList nests // Convert to array for more efficient in-place iteration
    let mutable aw = awIn
    let mutable i = 0
    while i < arr.Length do
        let nest', awTmp = UpdateNest2 arr[i] aw
        arr[i] <- nest' // Update array in-place
        aw <- awTmp // Update the mutable state
        i <- i + 1

    (Array.toList arr, aw) // Convert back to list for output

let UpdateWorld (aw: AntWorld) : AntWorld =
    let nests = aw.nests
    let trails2 = FadeTrails aw.trails
    let aw2 = { aw with trails = trails2 }
    //let nests2, aw3 = MonadicNestsUpdater nests aw2
    let nests2, aw3 = NestsUpdater nests aw2
    { aw2 with
        nests = nests2
        foodItems = aw3.foodItems
        trails = aw3.trails }
