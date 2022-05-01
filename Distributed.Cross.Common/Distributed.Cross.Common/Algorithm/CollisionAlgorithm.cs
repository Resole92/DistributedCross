using Distributed.Cross.Common.Algorithm.Utilities;
using Distributed.Cross.Common.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Algorithm
{
    public class CollisionAlgorithm
    {
        private CrossMap _map;
        private List<int> _collisionResults { get; set; } = new List<int>();
        private TrajectoryAlgorithm _trajectoryAlgorithm;

        public CollisionAlgorithm(CrossMap map)
        {
            _map = map;
        }

        public void Calculate()
        {

            _trajectoryAlgorithm = new TrajectoryAlgorithm(_map);
            _trajectoryAlgorithm.CreateGraphMatrixRappresentation();

            _collisionResults.Clear();

            var inputNodeVehicles = _map.Map.GetAllNodes().Where(node => node.Vehicle is not null && node.Type == CrossNodeType.Input);

            var inputVehicles = inputNodeVehicles.Select(node => node.Vehicle);
            var priorityGrouped = inputVehicles.GroupBy(vehicle => vehicle.Priority).OrderByDescending(x => x.Key);

            List<CollisionLayer> layers = new List<CollisionLayer>();
            foreach (var inputGroup in priorityGrouped)
            {
                var layer = new CollisionLayer
                {
                    Priority = inputGroup.Key,
                    Vehicles = inputGroup.Select(x => x.Identifier).ToList(),
                };

                layers.Add(layer);
            }

            foreach (var layer in layers)
            {
                //Collisions with highest layer
                var upperLayer = layers.Where(x => x.Priority > layer.Priority).ToList();
                UpperLayerCalculation(layer, upperLayer);

                //Collisions inside current layer
                InternalLayerCalculation(layer);
            }
        }

        private void UpperLayerCalculation(CollisionLayer layer, List<CollisionLayer> upperLayers)
        {
            var priorityVehicles = upperLayers.SelectMany(x => x.Vehicles);

            foreach(var vehicle in layer.Vehicles.ToList())
            {
                var vehicleTrajectory = _trajectoryAlgorithm.Calculate(vehicle);
                foreach (var priorityVehicle in priorityVehicles)
                {
                    var vehiclePriorityTrajectory = _trajectoryAlgorithm.Calculate(vehicle);

                    var collisions = vehicleTrajectory.Trajectory.Intersect(vehiclePriorityTrajectory.Trajectory);

                    if (collisions.Any())
                    {
                        layer.Vehicles.Remove(vehicle);
                        _collisionResults.Add(vehicle);
                        break;
                    }
                }
            }
        }

        private void InternalLayerCalculation(CollisionLayer layer)
        {
            var collisions = LayerCollisionCalculation(layer);

            while (collisions.Any())
            {
               
                var firstCollisionLayer = collisions.GroupBy(x => x.CollisionCounter).OrderByDescending(group => group.Key).First();
                if (firstCollisionLayer.Key > 0)
                {
                    //Take the ID which greater id and put on next layer
                    var majorVehichle = firstCollisionLayer.OrderByDescending(x => x.VehicleIdentifier).First();
                    var majorVechicleIdentifier = majorVehichle.VehicleIdentifier;

                    //Remove vehicle for this layer
                    collisions.Remove(majorVehichle);
                    layer.Vehicles.Remove(majorVechicleIdentifier);
                    _collisionResults.Add(majorVechicleIdentifier);

                    foreach (var collision in collisions)
                    {
                        //Remove collision on other vehicles
                        var collisionWithRemovedVehicle = collision.VehicleCollided.FirstOrDefault(x => x.SecondVehicleId == majorVechicleIdentifier);

                        collision.VehicleCollided.Remove(collisionWithRemovedVehicle);
                    }

                }
                else
                {
                    break;
                }
            }
        }

        private List<CollisionVehicles> LayerCollisionCalculation(CollisionLayer layer)
        {
            var trajectories = new List<TrajectoryResult>();
            var collisionsBewtweenVehicles = new List<CollisionVehicles>();
            foreach (var vehicle in layer.Vehicles)
            {
                var trajectory = _trajectoryAlgorithm.Calculate(vehicle);
                trajectories.Add(trajectory);
            }

            for (var first = 0; first < trajectories.Count; first++)
            {
                var firstTrajectory = trajectories[first];

                var firstVehicleCollisionFound = collisionsBewtweenVehicles.FirstOrDefault(x => x.VehicleIdentifier == firstTrajectory.Identifier);
                if (firstVehicleCollisionFound is null)
                {
                    firstVehicleCollisionFound = new CollisionVehicles
                    {
                        VehicleIdentifier = firstTrajectory.Identifier
                    };
                    collisionsBewtweenVehicles.Add(firstVehicleCollisionFound);
                }

                for (var second = first + 1; second < trajectories.Count; second++)
                {

                    var secondTrajectory = trajectories[second];

                    var secondVehicleCollisionFound = collisionsBewtweenVehicles.FirstOrDefault(x => x.VehicleIdentifier == secondTrajectory.Identifier);
                    if (secondVehicleCollisionFound is null)
                    {
                        secondVehicleCollisionFound = new CollisionVehicles
                        {
                            VehicleIdentifier = secondTrajectory.Identifier
                        };
                        collisionsBewtweenVehicles.Add(secondVehicleCollisionFound);
                    }

                    var collisions = firstTrajectory.Trajectory.Intersect(secondTrajectory.Trajectory);

                    if (collisions.Any())
                    {
                        firstVehicleCollisionFound.VehicleCollided.Add(new CollisionDetection
                        {
                            FirstVehicleId = firstTrajectory.Identifier,
                            SecondVehicleId = secondTrajectory.Identifier,
                            Collisions = collisions.ToList()
                        });
                        secondVehicleCollisionFound.VehicleCollided.Add
                            (new CollisionDetection
                            {
                                FirstVehicleId = secondTrajectory.Identifier,
                                SecondVehicleId = firstTrajectory.Identifier,
                                Collisions = collisions.ToList()
                            });
                    }
                }
            }

            return collisionsBewtweenVehicles;
        }

        public bool AmIRunner(int identifier)
        => _collisionResults.Any(x => x == identifier);
        

    }


}
