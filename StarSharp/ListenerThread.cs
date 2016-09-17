using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarSharp
{
	public enum ServerState
	{
		Starting = 0,
		ListenerReady = 1,
		StarboundReady = 2,
		Running = 3,
		Crashed = 4,
		Shutdown = 5,
		GracefulShutdown = 6,
	}

	class ListenerThread
	{
		public TcpListener tcpSocket;
		public Socket udpSocket;
		byte[] udpByteData = new byte[1024];

		Dictionary<EndPoint, byte[]> challengeData = new Dictionary<EndPoint, byte[]>();

		public void runTcp()
		{
			try
			{
				IPAddress localAdd = IPAddress.Parse(StarSharp.Instance.Config.ProxyIP);
				short port = StarSharp.Instance.Config.ProxyPort;
                tcpSocket = new TcpListener(localAdd, port);
				tcpSocket.Start();

				StarSharp.Instance.Utils.ConsoleInfo("Proxy server has been started on " + localAdd.ToString() + ":" + port);
				StarSharp.changeState(ServerState.ListenerReady, "ListenerThread::runTcp");
				try
				{
					while (true)
					{
						TcpClient clientSocket = tcpSocket.AcceptTcpClient();
						//new Thread(new ThreadStart(new Client(clientSocket).run)).Start();
					}
				}
				catch (ThreadAbortException)
				{
					Console.WriteLine("Thread has been aborted");
				}
				catch (Exception e)
				{
				}

				tcpSocket.Stop();
				Console.WriteLine("ListenerThread has failed - No new connections will be possible.");
			}
			catch (ThreadAbortException) { }
			catch (SocketException e)
			{
				Console.WriteLine("TcpListener has failed to start: " + e.Message);
			}
		}

		public void runUdp()
		{
			try
			{
				IPAddress localAdd = IPAddress.Parse(StarSharp.Instance.Config.ProxyIP);
				short proxyPort = StarSharp.Instance.Config.ProxyPort;
				udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				IPEndPoint ipEndPoint = new IPEndPoint(localAdd, proxyPort);

				udpSocket.Bind(ipEndPoint);

				IPEndPoint ipeSender = new IPEndPoint(IPAddress.Any, 0);
				//The epSender identifies the incoming clients
				EndPoint epSender = (EndPoint)ipeSender;

				Console.WriteLine("RCON listener has been started on UDP " + localAdd.ToString() + ":" + proxyPort);

				while (true)
				{
					int bytesRead = udpSocket.ReceiveFrom(udpByteData, ref epSender);

					Console.WriteLine("Receiving RCON Data...");
					OnReceive(udpByteData, bytesRead, epSender);
				}

			}
			catch (Exception e)
			{
				Console.WriteLine("Something went wrong while trying to setup the UDP listener. " + e.ToString());
			}
		}

		private void SourceRequest(byte[] data, EndPoint remote)
		{
			byte headerByte = data[4];
			byte[] dataArray;

			switch (headerByte)
			{
				case 0x54:
					dataArray = new byte[data.Length - 6];

					Buffer.BlockCopy(data, 5, dataArray, 0, dataArray.Length);

					string text = Encoding.UTF8.GetString(dataArray);
					string needle = "Source Engine Query";

					if (text != needle)
					{
						Console.WriteLine("RCON: Received invalid A2S_INFO request: " + text + " is invalid.");
						return;
					}
					else Console.WriteLine("RCON: Matched A2S_INFO request!");

					try
					{
						byte header = 0x49;
						byte protocol = 0x02;
						byte[] name = encodeString(StarSharp.Instance.Config.ServerName);
						byte[] map = encodeString("Starbound");
						byte[] folder = encodeString("na");
						byte[] game = encodeString("Starbound");
						byte[] appID = BitConverter.GetBytes(Convert.ToUInt16(1337));
						byte players = Convert.ToByte((uint)StarSharp.PlayersOnline);
						byte maxplayers = Convert.ToByte((uint)StarSharp.Instance.Config.MaxClients);
						byte bots = Convert.ToByte((uint)0);
						byte servertype = Convert.ToByte('d');
						byte environment = Convert.ToByte('w');
						byte visibility = Convert.ToByte((uint)(0));
						byte vac = Convert.ToByte((uint)0);
						byte[] version = encodeString("SomeVersion");

						var s = new MemoryStream();
						s.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 4);
						s.WriteByte(header);
						s.WriteByte(protocol);
						s.Write(name, 0, name.Length);
						s.Write(map, 0, map.Length);
						s.Write(folder, 0, folder.Length);
						s.Write(game, 0, game.Length);
						s.Write(appID, 0, appID.Length);
						s.WriteByte(players);
						s.WriteByte(maxplayers);
						s.WriteByte(bots);
						s.WriteByte(servertype);
						s.WriteByte(environment);
						s.WriteByte(visibility);
						s.WriteByte(vac);
						s.Write(version, 0, version.Length);

						Console.WriteLine("RCON: Sending A2S_INFO Response packet to " + remote);
						udpSocket.SendTo(s.ToArray(), remote);
					}
					catch (Exception e)
					{
						Console.WriteLine("RCON: Unable to send data to stream! An error occurred.");
						Console.WriteLine("RCON: " + e.ToString());
					}
					break;

				case 0x55:
					Console.WriteLine("RCON: Received A2S_PLAYER request from " + remote);

					dataArray = new byte[4];
					Buffer.BlockCopy(data, 5, dataArray, 0, dataArray.Length);

					if (dataArray.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }))
					{
						var buffer = new byte[4];
						new Random().NextBytes(buffer);

						if (challengeData.ContainsKey(remote)) challengeData.Remove(remote);
						challengeData.Add(remote, buffer);

						var s = new MemoryStream();
						s.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 4);
						s.WriteByte(0x41);
						s.Write(buffer, 0, 4);

						Console.WriteLine("RCON: Sending A2S_PLAYER Challenge Response packet to " + remote);
						udpSocket.SendTo(s.ToArray(), remote);
					}
					else
					{
						if (!challengeData.ContainsKey(remote)) Console.WriteLine("RCON: Illegal A2S_PLAYER request received from " + remote + ". No challenge number has been issued to this address.");
						else
						{
							var s = new MemoryStream();
							s.Write(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }, 0, 4);
							s.WriteByte(0x44);

							s.WriteByte(Convert.ToByte((uint)StarSharp.PlayersOnline));

							/*List<Client> clientList = StarryboundServer.getClients();

							for (var i = 0; i < clientList.Count; i++)
							{
								Client client = clientList[i];
								s.WriteByte(Convert.ToByte((uint)i));

								byte[] name = encodeString(client.playerData.name);
								s.Write(name, 0, name.Length);

								byte[] score = new byte[4];
								score = BitConverter.GetBytes((int)0);
								s.Write(score, 0, score.Length);

								float seconds = Utils.getTimestamp() - client.connectedTime;
								byte[] connected = new byte[4];
								connected = BitConverter.GetBytes(seconds);
								s.Write(connected, 0, connected.Length);

								//StarryboundServer.logDebug("ListenerThread::SourceA2SPlayer", "Client ID #" + i + ": " + Utils.ByteArrayToString(new byte[] { Convert.ToByte((uint)i) }) + Utils.ByteArrayToString(name) + Utils.ByteArrayToString(score) + Utils.ByteArrayToString(connected));
							}*/

							Console.WriteLine("RCON: Sending A2S_PLAYER Response packet for " + StarSharp.PlayersOnline + " player(s) to " + remote);
							//StarryboundServer.logDebug("ListenerThread::SourceA2SPlayer", "RCON: Dump packet: " + Utils.ByteArrayToString(s.ToArray()));
							udpSocket.SendTo(s.ToArray(), remote);
						}
					}
					break;

				default:
					Console.WriteLine("RCON: Received unknown or unsupported header byte - " + headerByte);
					break;
			}
		}

		private byte[] encodeString(string data)
		{
			return Encoding.UTF8.GetBytes(data + "\0");
		}

		private void OnReceive(byte[] dataBuffer, int bytesRead, EndPoint remote)
		{
			byte[] data = new byte[bytesRead];

			try
			{
				Buffer.BlockCopy(dataBuffer, 0, data, 0, bytesRead);

				/*
                 * Source Query packets begin with 0xFF (x4)
                 */

				if (bytesRead > 4)
				{
					byte[] sourceCheck = new byte[] { data[0], data[1], data[2], data[3] };

					if (sourceCheck.SequenceEqual(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }))
					{
						SourceRequest(data, remote);
						return;
					}
				}

				string text = Encoding.UTF8.GetString(data, 0, bytesRead);

				Console.WriteLine(String.Format("RCON: Received non-source request of {0} bytes from {1}: {2}", bytesRead, remote, text));
			}
			catch (Exception e)
			{
				Console.WriteLine("Bad RCON request received. " + e.ToString());
				Console.WriteLine("RCON: Binary data: " + Utils.ByteArrayToString(data));
			}
		}
	}
}
