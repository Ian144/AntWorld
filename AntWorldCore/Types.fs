module Types


[<Measure>]
type distance

[<Measure>]
type food

[<Measure>]
type health

[<Measure>]
type time

type IMyInterface =
    abstract GetValue: unit -> string

// x and y co-ordinates of a location
[<StructuralComparison>]
[<StructuralEquality>]
type Location =
    { x: float<distance>
      y: float<distance> }


// multiple types can be 'bumped into', IRadLoc enables the different 'bumpable' types to be treated in the same way
// todo - think of a better name for this
//        could use ^ generic params
type IRadLoc =
    abstract GetLoc: Location
    abstract GetRadius: float<distance>


// food item radius - amount food is proportional to area, which is a circle
// area  = pi.r^2 therefore r = sqrt(amountFood */PI)
type FoodItem =
    { loc: Location
      amountFood: int<food> }

    interface IRadLoc with
        member this.GetLoc = this.loc

        member this.GetRadius =
            let tmp = (float this.amountFood)
            LanguagePrimitives.FloatWithMeasure<distance>(sqrt (tmp / System.Math.PI))

type Obstacle =
    { radius: float<distance>
      loc: Location }

    interface IRadLoc with
        member this.GetLoc = this.loc
        member this.GetRadius = this.radius

// represents a change in location
type MoveVec =
    { dx: float<distance>
      dy: float<distance> }


// used in two modules
let antStepSize = 1.0<distance>



type AntState =
    | ReturnToNestHungary
    | ReturnToNestWithFood of int<food> // the amount of food being carried
    | SearchingForFood of MoveVec
    | FollowingTrail of MoveVec // the Ant needs direction (MoveVec) incase it reaches the end of the trail
    | InNest
    | DetectedFood of FoodItem
    | GettingUnStuck of AntState * int * MoveVec // holds an 'prev state', unstuck attempt count, and a MoveVec


type Ant =
    { foodStored: int<food>
      loc: Location
      prevLocs: Location list
      nestLoc: Location
      state: AntState }


let AntStateToString (ant: Ant) =
    match ant.state with
    | ReturnToNestHungary -> "ReturnToNestHungary"
    | ReturnToNestWithFood _ -> "ReturnToNestWithFood"
    | SearchingForFood _ -> "SearchingForFood"
    | FollowingTrail _ -> "FollowingTrail"
    | InNest -> "InNest"
    | DetectedFood _ -> "DetectedFood"
    | GettingUnStuck _ -> "GettingUnStuck"

type Nest =
    { Ants: Ant list
      FoodStore: int<food>
      Loc: Location }

    interface IRadLoc with
        member this.GetLoc = this.Loc
        member this.GetRadius = 4.0<distance>

type Trail = Map<Location, float>

type AntWorld =
    { nests: Nest list
      foodItems: FoodItem list
      trails: Trail
      obstacles: Obstacle list }
