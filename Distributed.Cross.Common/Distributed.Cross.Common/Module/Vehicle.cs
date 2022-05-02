﻿using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Enums;
using Distributed.Cross.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Distributed.Cross.Common.Module
{
    public class Vehicle
    {
        private RoundState _roundState;
        private int _leaderIdentifier;
        private int _actualPosition;
        private Logger _logger = new Logger();
        private CrossMap _map;
        private List<int> _vehicleRunner;

        public VehicleDto Data { get; private set; }
        private NodeActor _parentNode;



        public Vehicle(VehicleDto vehicleDto, CrossBuilder builder, NodeActor parentNode)
        {

            BuildMap(builder);

            Data = vehicleDto;
            _parentNode = parentNode;
            _map.AddVehicle(vehicleDto);
        }

        private void BuildMap(CrossBuilder builder)
        {
            builder.CreateBasicInputOutput();
            var crossMap = builder.Build();
            _map = crossMap;
        }

        public void RemoveParentNode()
        {
            _parentNode = null;
        }


        public void LeaderRequestAsk()
        {
            var totalInputLane = _map.Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Input).Count();

            var requests = new List<Task<LeaderElectionResponse>>();

            for (int vehicleId = _parentNode.Identifier + 1; vehicleId <= totalInputLane; vehicleId++)
            {
                var targetActor = _parentNode.ActorsMap[vehicleId];

                var request = targetActor.Ask<LeaderElectionResponse>(new LeaderElectionRequest(), TimeSpan.FromSeconds(2));
                requests.Add(request);
            }

            var someoneBetter = new List<object>();

            foreach (var request in requests)
            {
                try
                {
                    var result = request.Result;
                    if (result.Acknowledge)
                    {
                        someoneBetter.Add(result);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to request" + ex.Message);
                }
            }

            _logger.LogInformation($"I'm LEADER {_parentNode.Identifier}");
            _leaderIdentifier = _parentNode.Identifier;

            var requestsSubmitted = new List<Task<LeaderNotificationResponse>>();

            for (int vehicle = 1; vehicle <= totalInputLane; vehicle++)
            {

                if (vehicle == Data.StartLane) continue;
                var targetActor = _parentNode.ActorsMap[vehicle];
                var requestSubmitted = targetActor.Ask<LeaderNotificationResponse>(new LeaderNotificationRequest
                {
                    Identifier = _parentNode.Identifier
                }, TimeSpan.FromSeconds(2));

                requestsSubmitted.Add(requestSubmitted);
            }

            var vehiclesThatHaveResponse = new List<VehicleDto>();

            foreach (var requestSubmitted in requestsSubmitted)
            {
                try
                {
                    var result = requestSubmitted.Result;
                    var data = result.VehicleDetail;
                    _map.AddVehicle(data);
                    vehiclesThatHaveResponse.Add(data);
                }
                catch (Exception ex)
                {
                    _logger.LogError("Failed to request" + ex.Message);
                }
            }

            var vehicles = _map.Map.GetAllNodes().Where(node => node.Vehicle != null && node.Type == CrossNodeType.Input).Select(x => x.Vehicle);

            var coordinationDetail = new CoordinationNotificationRequest
            {
                VehiclesDetail = vehicles.ToList()
            };

            foreach (var vehicleThatHaveResponse in vehiclesThatHaveResponse)
            {
                var targetActor = _parentNode.ActorsMap[vehicleThatHaveResponse.StartLane];
                targetActor.Tell(coordinationDetail);
            };

            _parentNode.ActorsMap[Data.StartLane].Tell(coordinationDetail);
        }

        
        /// <summary>
        /// Return if is runner vehicle
        /// </summary>
        /// <param name="coordinationRequest"></param>
        /// <returns></returns>
        public bool CoordinationInformationReceive(CoordinationNotificationRequest coordinationRequest)
        {
            coordinationRequest.VehiclesDetail.ForEach(_map.AddVehicle);
            var collisionAlgorithm = new CollisionAlgorithm(_map);
            collisionAlgorithm.Calculate();
            var amIrunner = collisionAlgorithm.AmIRunner(Data.StartLane);

            var allRunner = new List<int>();

            foreach (var vehicle in coordinationRequest.VehiclesDetail)
            {
                if (collisionAlgorithm.AmIRunner(vehicle.StartLane)) allRunner.Add(vehicle.DestinationLane);
            }

            _vehicleRunner = allRunner;

            var isLeaderRunner = collisionAlgorithm.AmIRunner(_leaderIdentifier);
            if (isLeaderRunner) _leaderIdentifier = coordinationRequest.VehiclesDetail.First(x => x.StartLane == _leaderIdentifier).DestinationLane;

            if (amIrunner)
            {
                var destinationActor = _parentNode.ActorsMap[Data.DestinationLane];
                var startActor = _parentNode.ActorsMap[Data.StartLane];

                startActor.Tell(new VehicleRemoveNotification());

                destinationActor.Tell(new VehicleMoveNotification
                {
                    Vehicle = Data,
                    LeaderIdentifier = _leaderIdentifier,
                    AllVehicles = coordinationRequest.VehiclesDetail,
                    VehiclesRunning = allRunner
                }); 

                _logger.LogInformation($"Vehicle is crossing now from lane {Data.StartLane} to lane {Data.DestinationLane}");

            }
            else
            {
                _logger.LogInformation($"Vehicle NOT CROSSING from lane {Data.StartLane} to lane {Data.DestinationLane}...");
                collisionAlgorithm.IncrementPriority();
               
            }

            return amIrunner;
        }


        public void UpdateCrossingStatus(VehicleMoveNotification notification)
        {
            notification.AllVehicles.ForEach(_map.AddVehicle);
            _leaderIdentifier = notification.LeaderIdentifier;
            _vehicleRunner = notification.VehiclesRunning;

        }

        public VehicleDto LeaderElected(LeaderNotificationRequest request)
        {
            _leaderIdentifier = request.Identifier;
            return Data;
        }

        //Return true if round must be terminate
        public bool VehicleExit(int identifier)
        {
           
            _vehicleRunner.Remove(identifier);
            _logger.LogInformation($"Leader is notified about an exit vehicle with ID {identifier}");
            if (_vehicleRunner.Any()) return false;

            _logger.LogInformation("All vehicle left the cross. ROUND IS FINISH"!);


            var inputLanes = _map.Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Input).Select(x => x.Identifier);
            foreach(var inputLane in inputLanes)
            {
                var actorLane = _parentNode.ActorsMap[inputLane];
                actorLane.Tell(new ElectionStart());
            }

            return true;
            
        }


        public void StartCrossing()
        {
            Task.Run(() =>
            {
                _logger.LogInformation($"I'm moving {_parentNode.Identifier} on destination lane");
                Thread.Sleep(12000);
                var leaderActor = _parentNode.ActorsMap[_leaderIdentifier];
                leaderActor.Tell(new VehicleExitNotification
                {
                    Identifier = _parentNode.Identifier
                });
                _logger.LogInformation($"I have cross! I' am {_parentNode.Identifier}");

            });
        }

    }
}
