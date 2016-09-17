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
		internal int maxSimultaneousClientsThatWereConnected;

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

        public static Int32 mainSessionId = 1000000000;
		public static Int32 mainTransMissionId = 10000;

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

			SocketListenerSettings listenerSettings = new SocketListenerSettings(
				Instance.Config.MaxClients, 1, 100, 10, 4, 25, 4, 2, localEndPoint);
			SocketListener socketListener = new SocketListener(listenerSettings);

			while (true)
			{

			}
		}
	}
}
