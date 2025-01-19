module CollisionFuncs


open Types


// return true if the location is covered by an obstacle
let CollisionTest (loc: Location) (obs: Obstacle) =
    let dist = LocationFuncs.CalcDistance loc obs.loc
    dist <= obs.radius

let AnyCollisions (obstacles: Obstacle list) (locations: Location) =
    List.exists (CollisionTest locations) obstacles

let CollisionFilter (obstacles: Obstacle list) (locations: Location) = not (AnyCollisions obstacles locations)
