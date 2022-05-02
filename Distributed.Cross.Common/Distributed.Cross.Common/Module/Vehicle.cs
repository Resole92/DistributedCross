using Akka.Actor;
using Distributed.Cross.Common.Actors;
using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using Distributed.Cross.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public VehicleDto Data { get; private set; }
        private NodeActor _parentNode;



        public Vehicle(VehicleDto vehicleDto, CrossMap map, NodeActor parentNode)
        {
            _map = map;
            Data = vehicleDto;
            _parentNode = parentNode;
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

            var requestsSubmitted = new List<Task<LeaderNotificationResponse>>();

            for (int vehicle = 1; vehicle <= totalInputLane; vehicle++)
            {

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

            foreach (var vehicleThatHaveResponse in vehiclesThatHaveResponse)
            {
                var targetActor = _parentNode.ActorsMap[vehicleThatHaveResponse.StartLane];
                targetActor.Tell(new CoordinationNotificationRequest
                {
                    VehiclesDetail = vehicles.ToList()
                });
            };

        }

        public void CoordinationInformationReceive(CoordinationNotificationRequest coordinationRequest)
        => coordinationRequest.VehiclesDetail.ForEach(_map.AddVehicle);


        public VehicleDto LeaderElected(LeaderNotificationRequest request)
        {
            _leaderIdentifier = request.Identifier;
            return Data;
        }
    }
}
