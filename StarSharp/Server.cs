using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarSharp
{
	public class Server
	{
		private long _serverRunning;

		private int _numConnected;
		private int _totalJoined;

		protected TcpListener ListenSocket;

		public bool ServerRunning
		{
			get
			{
				return Interlocked.Read(ref _serverRunning) == 1;
			}
		}

		public IPEndPoint LocalEndPoint { get; private set; }

		public IPEndPoint ServerEndPoint { get; private set; }

		public bool AcceptingConnections { get; set; }

		public Server()
		{
			IPAddress bindAddress;
			IPAddress serverAddress;

			bindAddress = IPAddress.Parse(StarSharp.Instance.Config.ProxyIP);
			serverAddress = IPAddress.Parse("127.0.0.1");

			LocalEndPoint = new IPEndPoint(bindAddress, StarSharp.Instance.Config.ProxyPort);
			ServerEndPoint = new IPEndPoint(serverAddress, StarSharp.Instance.Config.ServerPort);

			_serverRunning = 0;
		}

		public virtual void StartServer()
		{
			if (ServerRunning)
				throw new Exception("Server is already running!");

			AcceptingConnections = true;

			ListenSocket = new TcpListener(LocalEndPoint.Address, LocalEndPoint.Port);
			//ListenSocket.Server.NoDelay = true;
			//ListenSocket.Server.ReceiveBufferSize = 2048;
			//ListenSocket.Server.SendBufferSize = 2048;

			try
			{
				ListenSocket.Start();
			}
			catch (Exception ex)
			{			}

			Interlocked.CompareExchange(ref _serverRunning, 1, 0);

			Task.Run(() => StartAccept());
		}
		protected virtual void StartAccept()
		{
			try
			{
				TcpClient client = ListenSocket.AcceptTcpClient();

				ProcessAccept(client);
			}
			catch
			{
			}

			if (ServerRunning)
				StartAccept();
		}

		protected void ProcessAccept(TcpClient client)
		{
			if (ListenSocket == null || !ServerRunning)
				return;

			if (!client.Connected)
				return;

			if (!AcceptingConnections)
			{
				client.Client.Shutdown(SocketShutdown.Send);
				client.Close();

				return;
			}

			Console.WriteLine("Connection from {0}", client.Client.RemoteEndPoint);

		/*	if (_numConnected >= ServerConfig.MaxConnections)
			{
				StarLog.DefaultLogger.Warn("Exceeded maximum amount of users! Disconnecting {0}", client.Client.RemoteEndPoint);

				//TODO: Simulate connnection, return error message to player

				client.Client.Shutdown(SocketShutdown.Send);
				client.Close();

				return;
			}

			Interlocked.Increment(ref _numConnected);
			Interlocked.Increment(ref _totalJoined);

			try
			{
				Thread proxyThread = new Thread(() =>
				{
					StarClientConnection cl = new StarClientConnection(client.Client, _packetTypes);
					cl.RegisterPacketHandlers(_packetHandlers.Select(p => p.Value()));

					StarServerConnection server = new StarServerConnection(_packetTypes);
					server.RegisterPacketHandlers(_packetHandlers.Select(p => p.Value()));

					var starProxy = new StarProxy(this, cl, server);
					starProxy.ConnectionClosed += (s, args) => Interlocked.Decrement(ref _numConnected);

					Proxies.AddProxy(starProxy.ConnectionId, starProxy);

					starProxy.Start();
				});

				proxyThread.IsBackground = true;
				proxyThread.Start();
			}
			catch (Exception ex)
			{
				ex.LogError();
			}*/
		}
	}
}
