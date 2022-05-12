using Distributed.Cross.Common.Communication.Messages;
using Distributed.Cross.Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Distributed.Cross.Common.Actors
{
    public partial class NodeActor
    {

        #region Broken behaviour

        public List<VehicleDto> BrokenNodes { get; private set; } = new List<VehicleDto>();

        public void BrokenBehaviour()
        {
            Receive<BrokenVehicleRequest>(message =>
            {
                var response = new BrokenVehicleResponse
                {
                    BrokenNodes = BrokenNodes.Select(x => x.BrokenNode.Value).ToList()
                };

                Sender.Tell(response, Self);
            });

            Receive<VehicleBrokenRemoveCommand>(message =>
            {
                var brokenNodeFound = BrokenNodes.FirstOrDefault(x => x.BrokenNode.Value == message.Identifier);
                if (brokenNodeFound is not null)
                {
                    BrokenNodes.Remove(brokenNodeFound);
                }

                SendBroadcastMessage(new VehicleExitNotification
                {
                    BrokenNode = message.Identifier
                });

                if (!BrokenNodes.Any())
                {
                    Become(IdleBehaviour);
                }
            });


            Receive<VehicleBrokenCommand>(message =>
            {
                var vehicle = message.Vehicle;
                _logger.LogInformation($"A vehicle that input from {vehicle.InputLane} lane and output to {vehicle.OutputLane} is broken on {vehicle.BrokenNode} node");
                if (!BrokenNodes.Any(x => x.BrokenNode.Value == vehicle.BrokenNode.Value))
                {
                    BrokenNodes.Add(vehicle);
                    SendBroadcastMessage(new VehicleBrokenNotification(message.Vehicle));
                }
            });
        }

        #endregion 

       
    }
}
