using System;
using MySql.Data.MySqlClient.Authentication;
using NHibernate;
using NHibernate.Mapping.ByCode;
using Photon.SocketServer;
using log4net;
using log4net.Config;
using System.IO;
using System.Collections.Generic;
using NHibernate.Cfg;
using RuneSlinger.Server.Entities;
using RuneSlinger.Server.ValueObject;
using Log4NetLoggerFactory = ExitGames.Logging.Log4Net.Log4NetLoggerFactory;

namespace RuneSlinger.Server
{
    public class Application : ApplicationBase
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Application));
        private readonly List<RunePeer> _peers;
        public IEnumerable<RunePeer> Peers { get { return _peers; } }
        private ISessionFactory _sessionFactory; 

        public Application()
        {
            _peers = new List<RunePeer>();
        }

        public void DestroyPeer(RunePeer peer)
        {
            _peers.Remove(peer);
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            var peer = new RunePeer(this, initRequest);
            _peers.Add(peer);
            return peer;
        }

        protected override void Setup()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(BinaryPath, "log4net.config")));
            ExitGames.Logging.LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            SetupNHibernate();

           

            Log.Info("Application Started!");
        }

        private void SetupNHibernate()
        {
            var config = new Configuration();
            config.Configure(); 

            var mapper = new ModelMapper();
            mapper.AddMapping<UserMap>();
            config.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
            _sessionFactory = config.BuildSessionFactory();
        }

        protected override void TearDown()
        {
            Log.Info("Application Ending...");
        }


        public ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }
    }
}