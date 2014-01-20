using System;
using System.Threading;
using log4net;
using Microsoft.SqlServer.Server;
using NHibernate.Linq;
using NHibernate.Mapping;
using Photon.SocketServer;
using RuneSlinger.Base;
using System.Collections.Generic;
using System.Linq;
using RuneSlinger.Server.Entities;
using RuneSlinger.Server.ValueObject;

namespace RuneSlinger.Server
{
    public class RunePeer : PeerBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Application));
        private readonly Application _application;


        public RunePeer(Application application, InitRequest initRequest)
            : base(initRequest.Protocol, initRequest.PhotonPeer)
        {
            _application = application;
            Log.InfoFormat("Peer created at {0}:{1}", initRequest.RemoteIP, initRequest.RemotePort);
        }


        protected override void OnDisconnect(PhotonHostRuntimeInterfaces.DisconnectReason reasonCode, string reasonDetail)
        {
            Log.InfoFormat("Peer disconnected: {0}, {1}", reasonCode, reasonDetail);
            _application.DestroyPeer(this);
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            Log.Info("HIT ON_OPERATION_REQUEST");
            using (var session = _application.OpenSession())
            {
                using (var trans = session.BeginTransaction())
                {
                    try
                    {
                        Log.Info("Begun Transaction!");
                        var opCode = (RuneOperationCode)operationRequest.OperationCode;
                        if (opCode == RuneOperationCode.Register)
                        {

                            var username = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Username];
                            var email = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Email];
                            var password = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Password];
                            Register(session, username, password, email);
                        }
                        else if (opCode == RuneOperationCode.Login)
                        {
                            var email = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Email];
                            var password = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Password];
                            Login(session, password, email);
                        }
                        else if (opCode == RuneOperationCode.SendMessage)
                        {
                            var message = (string)operationRequest.Parameters[(byte)RuneOperationCodeParameter.Message];
                            SendMessage(session, message);
                        }
                        else
                        {
                            SendOperationResponse(new OperationResponse((byte)RuneOperationResponse.Invalid), sendParameters);
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        SendOperationResponse((new OperationResponse((byte)RuneOperationResponse.FatalError)), sendParameters);
                        trans.Rollback();
                        Log.ErrorFormat("ERROR processing operarion {0}: {1}", operationRequest.OperationCode, ex.Message);
                    }

                }
            }
        }

        private void SendMessage(NHibernate.ISession session, string message)
        {

        }

        private void Login(NHibernate.ISession session, string password, string email)
        {
            var user = session.Query<User>().SingleOrDefault(x => x.Email == email);
            if (user == null || !user.Password.EqualsPlainText(password))
            {
                SendError("Username or password is incorrect!");
                return; 
            }

            SendSuccess();

        }

        private void Register(NHibernate.ISession session, string username, string password, string email)
        {
            Log.Info("HIT REGISTER FUNCTION");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email))
            {
               SendError("All Fields Are required!");
                return;
            }

            if (username.Length > 128)
            {
                SendError("Username must be less than 128 characters!");
                return;
            }

            if (email.Length > 200)
            {
                SendError("Email must be less than 200 characters!");
                return;
            }

            if (session.Query<User>().Any(t => t.Username == username || t.Email == email))
            {
                SendError("Username and email must be unique!");
                return;
            }

            var user = new User
            {
                Email = email,
                CreatedAt = DateTime.UtcNow,
                Username = username,
                Password = HashedPassword.FromPlainText(password)
            };
            session.Save(user);
            SendSuccess();
        }

        private void SendSuccess()
        {
            SendOperationResponse(new OperationResponse(
                (byte)RuneOperationResponse.Success),
                new SendParameters()
                {
                    Unreliable = false
                });
        }

        private void SendError(string message)
        {
            SendOperationResponse(new OperationResponse(
                (byte)RuneOperationResponse.Error,
                new Dictionary<byte, object>
                {
                    {(byte) RuneOperationResponseParameter.ErrorMessage, message}
                }),
                new SendParameters()
                {
                    Unreliable = false
                });
        }
    }
}
