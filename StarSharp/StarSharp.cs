using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		internal int ParentProcessId;

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
		public static Thread ServerThread;

		static void Main(string[] args)
		{
			string[] lines = System.IO.File.ReadAllLines("title");
			foreach (string line in lines)
			{
				Console.WriteLine("\t" + line);
			}
			Instance.Utils.CustomText("\t\t   " + $"Version {Version} - {Edition} - \n\n\n", ConsoleColor.DarkGreen);
			Instance.Utils.ConsoleInfo("Initializing StarSharp server...");

			if (File.Exists("starbound_server.pid"))
			{
				int processId = Convert.ToInt32(File.ReadAllText("starbound_server.pid"));
				Process proc = null;
                try
				{
					proc = Process.GetProcessById(processId);
				}
				catch{ }
				if (proc != null)
				{
					proc.Kill();
				}
				File.Delete("starbound_server.pid");
			}

			Instance.Config = Config.Load();

			if (Instance.Config.ProxyPort == Instance.Config.ServerPort)
			{
				Instance.Utils.ConsoleError("You cannot have the serverPort and proxyPort on the same port!");
				Thread.Sleep(5000);
				Environment.Exit(3);
			}

			IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse(Instance.Config.ProxyIP), Instance.Config.ProxyPort);

			SocketListener socketListener = new SocketListener(Instance.Config.MaxClients, Instance.Config.BufferSize);
			socketListener.Start(localEndPoint.Port);

			MainServer = new ServerThread();
			ServerThread = new Thread(new ThreadStart(MainServer.Run));
			ServerThread.Start();


			while (true)
			{

			}
		}
	}
}
