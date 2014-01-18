using log4net;
using Photon.SocketServer;
using System.Collections.Generic;
using System.Linq;

namespace RuneSlinger.Server
{
    public class RunePeer : PeerBase
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Application));
        private readonly Application _application;


        public RunePeer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);
        }


        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            log.InfoFormat("Peer disconnected: {0}, {1}", reasonCode, reasonDetail);
            _application.DestroyPeer(this);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            if (operationRequest.OperationCode != 1)
            {
                log.WarnFormat("Peer send unknown code: {0}", operationRequest.OperationCode);
                return;
            }
            var message = (string)operationRequest.Parameters[0];
            log.DebugFormat("Got Message from client: {0}", message);

            
            var eventData = new EventData(
                    0, 
                    new Dictionary<byte, object>()
                    { 
                        {0,message}
                    });
            var sendParam = new SendParameters() 
                    {
                        Unreliable = false
                    };

            foreach (var peer in _application.Peers.Where(x => x != this))
            {
                peer.SendEvent(
                     eventData,
                     sendParam);
            }
          
        }
    }
}
