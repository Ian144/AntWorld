[<RequireQualifiedAccess>]
module FoodFuncs


open Types
open Utilities



let AntMaxFoodItemCarryable = 1000<food>


let CalcFoodItemRadius (foodItem:FoodItem) : float<distance>  = 
    let rl = foodItem :> IRadLoc
    rl.GetRadius


let IsWithinCircle (loc:Location) (circleLoc:Location) (circleRadius:float<distance>) : bool = 
    let distance = LocationFuncs.CalcDistance loc circleLoc
    distance <= circleRadius



let IsFoodDetected (antLoc:Location) (foodItem:FoodItem) : bool =
    let detectionDistance = 32.0<distance> // todo make detection distance a parameter
    let foodItemRadius = CalcFoodItemRadius foodItem
    let detectionRadius = foodItemRadius + detectionDistance
    IsWithinCircle antLoc foodItem.loc detectionRadius



let HasReachedFood loc foodItem = 
    let radius = CalcFoodItemRadius foodItem
    IsWithinCircle loc foodItem.loc radius



// lazyily return the first food item detected (or none)
let FoodDetected (loc:Location) (foodItems:FoodItem list) = 
    foodItems |> Seq.filter (IsFoodDetected loc) |> FirstOrNone



let TakeFood ant foodItem =
    let amountToTake = if foodItem.amountFood > AntMaxFoodItemCarryable then 
                           AntMaxFoodItemCarryable
                       else 
                           foodItem.amountFood
    let ant' = { ant with state = ReturnToNestWithFood amountToTake }
    let foodItem' = {foodItem with amountFood = foodItem.amountFood - amountToTake}
    (ant', foodItem')



// if some food has been take by an ant then the antWorlds foodItem list needs to be updated.
// creates a new footItem list from the old list and the updated item
let UpdateFoodItemList (foodList:FoodItem list) (updatedFoodItem:FoodItem) = 
    let SelectIfUpdate (item:FoodItem) =    if item.loc = updatedFoodItem.loc then
                                                updatedFoodItem
                                            else
                                                item
    // slot the updated fooditem into the list and remove any fooditems of zero size
    foodList |> List.map SelectIfUpdate |> List.filter (fun fdItem -> fdItem.amountFood > 0<food>)
