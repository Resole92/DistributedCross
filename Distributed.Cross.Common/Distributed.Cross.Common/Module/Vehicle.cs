using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Algorithm;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
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

        private int _leaderIdentifier;
        private Logger _logger;
        private CrossMap _map;
        private List<int> _vehicleRunnerLeft;
        private List<int> _vehicleRunner;

        public VehicleDto Data { get; private set; }
        private NodeActor _parentNode;




        public Vehicle(VehicleDto vehicleDto, CrossBuilder builder, NodeActor parentNode, Logger logger)
        {
            _logger = logger;
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

        public ElectionResult LeaderRequestAsk(CancellationToken token)
        {
            #region Request bully

            _logger.LogInformation($"A leader election algorithm is started");

            var totalInputLane = _map.Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Input).Count();


            var totalElectionRequests = _map.Map.GetAllNodes().Where(x => x.Type == CrossNodeType.Input || x.Type == CrossNodeType.Output).Count();
            var requests = new List<(int, Task<LeaderElectionResponse>)>();

            var self = _parentNode.ActorsMap[_parentNode.Identifier];

            for (int vehicleId = _parentNode.Identifier + 1; vehicleId <= totalElectionRequests; vehicleId++)
            {

                var targetActor = _parentNode.ActorsMap[vehicleId];
                var request = targetActor.Ask<LeaderElectionResponse>(new LeaderElectionRequest
                {
                    Identifier = _parentNode.Identifier
                }, TimeSpan.FromSeconds(Const.MaxTimeout));
                requests.Add((vehicleId, request));
            }

            _logger.LogInformation($"Start check responses");

            var someoneBetter = new List<int>();

            foreach (var request in requests)
            {
                try
                {
                    var result = request.Item2.Result;
                    if (result.Acknowledge)
                    {

                        someoneBetter.Add(result.Identifier);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to request bully presence to identifier {request.Item1}\n" + ex.Message);
                }
            }


            if (token.IsCancellationRequested) return new ElectionResult(ElectionResultType.Cancelled);

            var isSomeoneCrossing = someoneBetter.Any(x => x > totalInputLane);

            if (isSomeoneCrossing)
            {
                var crossings = someoneBetter.Where(x => x > totalInputLane);

                _logger.LogInformation($"Find some one that is crossing that are {string.Join(",", crossings)}");
                return new ElectionResult(ElectionResultType.Crossing);
            }

            var isSomeoneResponse = someoneBetter.Any();
            if (isSomeoneResponse)
            {
                _logger.LogInformation($"Find some one better that are {string.Join(",", someoneBetter)}");
                return new ElectionResult(ElectionResultType.Bully);
            }

            #endregion

            #region Request to be a leader

            _logger.LogInformation($"I'm LEADER! Try to notify!");

            //Ask  also about broken nodes 

            var requestsSubmitted = new List<(int, Task<LeaderNotificationResponse>)>();

            for (int vehicle = 1; vehicle <= totalInputLane; vehicle++)
            {
                if (vehicle == _parentNode.Identifier) continue;
                var targetActor = _parentNode.ActorsMap[vehicle];
                var requestSubmitted = targetActor.Ask<LeaderNotificationResponse>(new LeaderNotificationRequest
                {
                    Identifier = _parentNode.Identifier
                }, TimeSpan.FromSeconds(Const.MaxTimeout));

                requestsSubmitted.Add((vehicle, requestSubmitted));
            }

            var requestBrokenNodes = _parentNode.ActorsMap[Const.BrokenIdentifier].Ask<BrokenVehicleResponse>(new BrokenVehicleRequest
            {
                Identifier = _parentNode.Identifier
            }, TimeSpan.FromSeconds(Const.MaxTimeout));

            _map.EraseMapFromVehicles();
            _map.AddVehicle(Data);

            var vehiclesThatHaveResponse = new List<VehicleDto>();

            foreach (var requestSubmitted in requestsSubmitted)
            {
                try
                {
                    var result = requestSubmitted.Item2.Result;
                    if (!result.Acknowledge)
                    {
                        _logger.LogInformation($"Some leader is already present for node {result.VehicleDetail.InputLane}");
                        return new ElectionResult(ElectionResultType.LeaderAlreadyPresent);
                    }
                    var data = result.VehicleDetail;
                    _map.AddVehicle(data);
                    vehiclesThatHaveResponse.Add(data);
                    _logger.LogInformation($"Vehicle {result.VehicleDetail.InputLane} response for coordination");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to request vehicle details for vehicle {requestSubmitted.Item1}\n" + ex.Message);
                }
            }

            _logger.LogInformation($"OK I'm definitely the LEADER");

            #endregion

            #region Broken node

            var brokenNode = new List<int>();

            try
            {

                var result = requestBrokenNodes.Result;
                brokenNode = result.BrokenNodes;

            }
            catch (Exception ex)
            {
                _logger.LogError($"No leader of broken node present \n" + ex.Message);
            }

            #endregion

            #region Coordination notification


            _leaderIdentifier = _parentNode.Identifier;

            var vehicles = _map.Map.GetAllNodes().Where(node => node.Vehicle != null && node.Type == CrossNodeType.Input).Select(x => x.Vehicle);

            var coordinationDetail = new CoordinationNotification
            {
                VehiclesDetail = vehicles.Select(x => x.Clone()).ToList(),
                BrokenNodes = brokenNode
            };

            foreach (var vehicleThatHaveResponse in vehiclesThatHaveResponse)
            {
                var targetActor = _parentNode.ActorsMap[vehicleThatHaveResponse.InputLane];
                targetActor.Tell(coordinationDetail);
            };

            _parentNode.ActorsMap[Data.InputLane].Tell(coordinationDetail);

            return new ElectionResult(ElectionResultType.Elected, vehicles.Select(x => x.Clone()).ToList(), brokenNode);

            #endregion 

        }


        /// <summary>
        /// Return if is runner vehicle
        /// </summary>
        /// <param name="coordinationRequest"></param>
        /// <returns></returns>
        public bool CoordinationInformationReceive(CoordinationNotification coordinationRequest)
        {
            _map.EraseMapFromVehicles();

            coordinationRequest.BrokenNodes.ForEach(_map.AddBrokenNode);
            coordinationRequest.VehiclesDetail.ForEach(_map.AddVehicle);

            var collisionAlgorithm = new CollisionAlgorithm(_map);
            collisionAlgorithm.Calculate();
            var amIrunner = collisionAlgorithm.AmIRunner(Data.InputLane);

            var allRunner = new List<int>();

            foreach (var vehicle in coordinationRequest.VehiclesDetail)
            {
                if (collisionAlgorithm.AmIRunner(vehicle.InputLane)) allRunner.Add(vehicle.OutputLane);
            }

            _vehicleRunnerLeft = allRunner;
            _vehicleRunner = _vehicleRunnerLeft.ToList();

            var isLeaderRunner = collisionAlgorithm.AmIRunner(_leaderIdentifier);
            if (isLeaderRunner) _leaderIdentifier = coordinationRequest.VehiclesDetail.First(x => x.InputLane == _leaderIdentifier).OutputLane;

            if (amIrunner)
            {
                var destinationActor = _parentNode.ActorsMap[Data.OutputLane];
                var startActor = _parentNode.ActorsMap[Data.InputLane];
                startActor.Tell(new VehicleRemoveCommand());

                destinationActor.Tell(new VehicleMoveCommand
                {
                    CrossNode = collisionAlgorithm.GetTrajectory(_parentNode.Identifier).Trajectory. Union(new List<int> { _parentNode.Identifier }).ToList(),
                    Vehicle = Data.Clone(),
                    LeaderIdentifier = _leaderIdentifier,
                    AllVehicles = coordinationRequest.VehiclesDetail,
                    VehiclesRunning = allRunner
                });

                _logger.LogInformation($"Vehicle is crossing now from lane {Data.InputLane} to lane {Data.OutputLane}");

            }
            else
            {
                _logger.LogInformation($"Vehicle NOT CROSSING from lane {Data.InputLane} to lane {Data.OutputLane}...");
                Data.Priority++;
            }

            return amIrunner;
        }

        public void UpdateCrossingStatus(VehicleMoveCommand notification)
        {
            notification.AllVehicles.ForEach(_map.AddVehicle);
            _leaderIdentifier = notification.LeaderIdentifier;
            _vehicleRunnerLeft = notification.VehiclesRunning.ToList();
            _vehicleRunner = _vehicleRunnerLeft.ToList();
        }

        public LeaderNotificationResponse LeaderElected(LeaderNotificationRequest request)
        {
            if (request.Identifier > _parentNode.Identifier)
            {
                _leaderIdentifier = request.Identifier;
                return new LeaderNotificationResponse
                {
                    Acknowledge = true,
                    VehicleDetail = Data.Clone()
                };
            }

            return new LeaderNotificationResponse
            {
                Acknowledge = false,
                VehicleDetail = Data.Clone()
            };
        }

        //Return true if round must be terminate
        public void CheckEndRound(VehicleExitNotification message)
        {
            if (_parentNode.Identifier != _leaderIdentifier) return;

            _vehicleRunnerLeft.Remove(message.Identifier);
            _logger.LogInformation($"Leader is notified about an exit vehicle with ID {message.Identifier}");

            if (_vehicleRunnerLeft.Any()) return;
            _logger.LogInformation("All vehicle left the cross. ROUND IS FINISH"!);

            var self = _parentNode.ActorsMap[_leaderIdentifier];
            self.Tell(new RoundEndNotification());
            

        }

        public void EndRound(RoundEndNotification endRoundNotification)
        {



            var self = _parentNode.ActorsMap[Data.OutputLane];

            var leaderPresentOnCrossing = _vehicleRunner.Any(x => x == _parentNode.Identifier);
            if (leaderPresentOnCrossing)
            {
                _logger.LogInformation($"I'm LEADER and I left the cross! Bye bye");

                 self.Tell(new VehicleRemoveCommand());
            }

            //Remove must be done before, otherwise new node can send a message of request acutal busy node before removing.
            _parentNode.SendBroadcastMessage(new ElectionStart
            {
                LastRoundVehicleRunning = _vehicleRunner.ToList(),
            });

        }


        public void StartCrossing()
        {
            var parentNode = _parentNode;


            Task.Run(() =>
            {
                _logger.LogInformation($"I'm starting from {Data.InputLane} and moving on destination lane {Data.OutputLane}");
                Thread.Sleep((int)(Data.Speed * 1000));
                var self = parentNode.ActorsMap[Data.OutputLane];

                var leaderActor = parentNode.ActorsMap[_leaderIdentifier];
                _logger.LogInformation($"I have cross!");
                if (parentNode.Identifier != _leaderIdentifier)
                {
                     self.Tell(new VehicleRemoveCommand());
                }
                else
                {
                    self.Tell(new VehicleExitNotification 
                    {
                        Identifier = _leaderIdentifier 
                    });
                }
            });
        }

    }
}
