using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Test
{
    public class CollisionEnvironment
    {
        public bool Test(CollisionDataTest data)
        {
            var builder = data.Builder;
            var crossMap = builder.Build();

            data.BrokenNodes.ForEach(crossMap.AddBrokenNode);

            var groupVehicle = data.Vehicles.GroupBy(vehicle => vehicle.InputLane);
            var dictionaries = new Dictionary<int, Queue<VehicleDto>>();
            foreach(var group in groupVehicle)
            {
                var queue = new Queue<VehicleDto>();
                group.ToList().ForEach(queue.Enqueue);
                dictionaries.Add(group.Key, queue);
            }

            var roundVehicles = new List<VehicleDto>();
            

            foreach(var queue in dictionaries.Values)
            {
                var vehicle = queue.Dequeue();
                crossMap.AddVehicle(vehicle);
                roundVehicles.Add(vehicle);
            }

            var roundNumber = 1;

            while(dictionaries.Any())
            {
                var collisionAlgorithm = new CollisionAlgorithm(crossMap);
                collisionAlgorithm.Calculate();

                var roundDto = new RoundDto
                {
                    Number = roundNumber
                };

                foreach (var roundVehicle in roundVehicles)
                {

                    var vehicleId = roundVehicle.InputLane;
                    var isRunner = collisionAlgorithm.AmIRunner(vehicleId);
                    if (isRunner)
                    {
                        crossMap.RemoveVehicle(vehicleId);
                        roundDto.VehiclesRunning.Add(vehicleId);
                    }
                    else
                    {
                        roundDto.VehiclesNotRunning.Add(vehicleId);
                        var queue = dictionaries[vehicleId];
                        queue.Enqueue(roundVehicle);
                    }
                }

                foreach (var vehicleRun in roundDto.VehiclesRunning)
                {
                    var queue = dictionaries[vehicleRun];
                    if (queue.Any())
                    {
                        var vehicle = queue.Dequeue();
                        crossMap.AddVehicle(vehicle);
                    }
                    else
                    {
                        dictionaries.Remove(vehicleRun);
                    }
                }

                //Check if round is ok

                var expectedRound = data.Rounds.First(round => round.Number == roundNumber);
                if (!expectedRound.IsSameRound(roundDto)) return false;

                roundNumber++;

            }

            return true;
           
            
        }


        

    }

    public class CollisionDataTest
    {
        public CrossBuilder Builder { get; set; }
        public List<int> BrokenNodes { get; set; } = new List<int>();
        public List<VehicleDto> Vehicles { get; set; }
        public List<RoundDto> Rounds { get; set; }
        public bool MustBeTrue { get; set; }


    }
}
