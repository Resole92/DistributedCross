using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Algorithm.Utilities
{


    public class CollisionVehicles
    {
        public int VehicleIdentifier { get; set; }
        public int NumberVehiclesCollision { get; set; }
        public List<CollisionDetection> VehicleCollided { get; set; }

        public bool IsHappenCollision => VehicleCollided.Any();
        public int CollisionCounter => VehicleCollided.Count();

    }


    public class CollisionDetection
    {
        public int FirstVehicleId { get; set; }
        public int SecondVehicleId { get; set; }
        public List<int> Collisions { get; set; } = new List<int>();
        public bool IsHappenCollision => Collisions.Any();
        public int CollisionCounter => Collisions.Count();
    }


    public class CollisionLayer
    {
        public int Priority { get; set; }
        public List<int> Vehicles { get; set; } = new List<int>();
    }
}
