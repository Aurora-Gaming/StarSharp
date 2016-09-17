using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StarSharp
{
	class ServerThread
	{
		public Process process;

		public void run()
		{
			var executableName = "starbound_server.exe";
			try
			{
				ProcessStartInfo startInfo = new ProcessStartInfo(executableName)
				{
					WindowStyle = ProcessWindowStyle.Hidden,
					UseShellExecute = false,
					RedirectStandardOutput = true,
					CreateNoWindow = true
				};
				process = Process.Start(startInfo);
				//StarSharp.Instance.ParentProcessId = process.Id;
				//File.WriteAllText("starbound_server.pid", process.Id.ToString());
				process.OutputDataReceived += (sender, e) => Console.WriteLine(e.Data);
				process.ErrorDataReceived += (sender, e) => Console.WriteLine("ErrorDataReceived from starbound_server.exe: " + e.Data);
				process.BeginOutputReadLine();
				process.WaitForExit();
}
			catch (ThreadAbortException) { }
			catch (Exception e)
			{
			}
		}
	}
}
