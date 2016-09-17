using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarSharp
{
	class Utils
	{
		public void ConsoleInfo(string text)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.Gray;
		}
		public void ConsoleError(string text, object args = null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.WriteLine(text, args);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public void CustomText(string text, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(text);
			Console.ForegroundColor = ConsoleColor.Gray;
		}

		public static string ByteArrayToString(byte[] buffer)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach (byte b in buffer)
				sb.Append(b.ToString("X2"));

			return (sb.ToString());
		}
	}
}
