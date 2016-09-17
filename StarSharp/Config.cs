using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSharp
{
	class Config
	{
		public short ServerPort { get; set; } = 21024;
		public string ProxyIP { get; set; } = "0.0.0.0";
		public short ProxyPort { get; set; } = 21025;
		public int MaxClients { get; set; } = 25;
		public string ServerName { get; set; } = "StarSharp Server";
		public int BufferSize { get; set; } = 2048;

		public static Config Load()
		{
			try
			{
				Config config = new Config();
				if (File.Exists(StarSharp.Instance.ConfigPath))
				{
					config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(StarSharp.Instance.ConfigPath));
				}
				config.Save();
				return config;
			}
			catch (Exception ex)
			{
				StarSharp.Instance.Utils.ConsoleError("Failed to load config! \n {0}", ex);
				return null;
			}
		}

		public void Save()
		{
			File.WriteAllText(StarSharp.Instance.ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
		}
	}
}
