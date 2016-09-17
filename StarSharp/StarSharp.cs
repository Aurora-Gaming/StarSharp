using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarSharp
{
	class StarSharp
	{
		private static readonly StarSharp _instance = new StarSharp();
		public static StarSharp Instance
		{
			get
			{
				return _instance;
			}
		}
		public Utils Utils = new Utils();

		public static string Version
		{
			get { return "v1.0"; }
		}
		public static string Edition
		{
			get { return "*-* Babysteps Edition *-*"; }
		}

		public string ConfigPath
		{
			get { return "config.json"; }
		}
		public string MotdPath
		{
			get { return "motd.txt"; }
		}
		public string RulesPath
		{
			get { return "rules.txt"; }
		}
		public string WhiteListPath
		{
			get { return "motd.txt"; }
		}

		public Config Config
		{
			get; set;
		}

		public static ServerThread MainServer;
		public static ListenerThread Listener;
		static Thread SbServerThread;
		static Thread ListenerThread;
		static Thread UdpThread;
		public static ServerThread sbServer;
		static Thread sbServerThread;
		public int parentProcessId;

		public static int PlayersOnline;
		private static ServerState aServerState;
		public static ServerState serverState { get { return aServerState; } set { return; } }

		public static void changeState(ServerState aState, string caller, string reason = "Not Specified")
		{
			string format = "StateChange requested by {0} to {1}: {2}";

			switch (aState)
			{
				case ServerState.Crashed:
					Console.WriteLine(string.Format(format, caller, aState, reason));
					break;

				case ServerState.GracefulShutdown:
					Console.WriteLine(string.Format(format, caller, aState, reason));
					break;

				default:
					Console.WriteLine("StarryboundServer::changeState", string.Format(format, caller, aState, reason));
					break;
			}

			aServerState = aState;
		}

		static void Main(string[] args)
		{
			string[] lines = System.IO.File.ReadAllLines("title");
			foreach (string line in lines)
			{
				Console.WriteLine("\t" + line);
			}
			Instance.Utils.CustomText("\t\t   " + $"Version {Version} - {Edition} - \n\n\n", ConsoleColor.DarkGreen);
			Instance.Utils.ConsoleInfo("Initializing StarSharp server...");

			Instance.Config = Config.Load();

			if (Instance.Config.ProxyPort == Instance.Config.ServerPort)
			{
				Instance.Utils.ConsoleError("You cannot have the serverPort and proxyPort on the same port!");
				Thread.Sleep(5000);
				Environment.Exit(3);
			}

			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(Instance.Config.ProxyIP), Instance.Config.ProxyPort);

			Listener = new ListenerThread();
			ListenerThread = new Thread(new ThreadStart(Listener.runTcp));
			ListenerThread.Start();

			UdpThread = new Thread(new ThreadStart(Listener.runUdp));
			UdpThread.Start();
			while (serverState != ServerState.ListenerReady) { }

			sbServer = new ServerThread();
			sbServerThread = new Thread(new ThreadStart(sbServer.run));
			sbServerThread.Start();

			while (true)
			{

			}
		}
	}
}
