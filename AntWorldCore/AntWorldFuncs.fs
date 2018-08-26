module AntWorldFuncs


open Types
open NestFuncs
open FSharpx.State
open PheromoneTrails




let rec MonadicNestsUpdater (nests: Nest list) = 
    state{  if nests.IsEmpty then
                return []
            else
                let! antWorld = getState
                let nest', antWorld2 = UpdateNest2 nests.Head antWorld
                do! putState antWorld2
                let! nests' = MonadicNestsUpdater nests.Tail 
                return nest' :: nests' }



let UpdateWorld (aw:AntWorld) : AntWorld = 
        let foodItems = aw.foodItems
        let nests = aw.nests
        let aw2 = {aw with trails = FadeTrails aw.trails} 
        let nests2, aw3 = MonadicNestsUpdater nests aw2
        {aw2 with nests = nests2; foodItems = aw3.foodItems; trails = aw3.trails} 
        

let OptUpdateWorld (aw:AntWorld) = Some (aw , (UpdateWorld aw))
