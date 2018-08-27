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


let NestUpdater (nest:Nest, aw:AntWorld) : (Nest*AntWorld) =
     UpdateNest2 nest aw


let NestsUpdater (nests:Nest list) (awIn:AntWorld) : (Nest list*AntWorld) =
    let mutable aw = awIn // is this uglier than the state monad
    let nests2 = 
        [   for nest in nests do
            let nest', awTmp = UpdateNest2 nest aw
            aw <- awTmp
            yield nest'
        ]
    (nests2, aw)




let UpdateWorld (aw:AntWorld) : AntWorld =
    let nests = aw.nests
    let trails2 = FadeTrails aw.trails
    let aw2 = {aw with trails = trails2} 
    //let nests2, aw3 = MonadicNestsUpdater nests aw2
    let nests2, aw3 = NestsUpdater nests aw2
    {aw2 with nests = nests2; foodItems = aw3.foodItems; trails = aw3.trails} 
        


