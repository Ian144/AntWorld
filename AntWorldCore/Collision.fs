module CollisionFuncs


open Types


// return true if the location is covered by an obstacle
let CollisionTest (loc:Location) (obs:Obstacle) = 
    let dist = LocationFuncs.CalcDistance loc obs.loc
    dist <= obs.radius


let AnyCollisions (obs:Obstacle list) (loc:Location) = List.exists (CollisionTest loc) obs


let CollisionFilter (obs:Obstacle list) (loc:Location) = not (AnyCollisions obs loc)


