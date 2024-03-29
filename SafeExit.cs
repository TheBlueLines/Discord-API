﻿using System.Net.WebSockets;
using System.Runtime.InteropServices;

namespace TTMC.Discord
{
	public class SafeExit
	{
		public static List<ClientWebSocket> webSockets = new();
		// https://docs.microsoft.com/en-us/windows/console/setconsolectrlhandler?WT.mc_id=DT-MVP-5003978
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(SetConsoleCtrlEventHandler handler, bool add);

		// https://docs.microsoft.com/en-us/windows/console/handlerroutine?WT.mc_id=DT-MVP-5003978
		private delegate bool SetConsoleCtrlEventHandler(CtrlType sig);

		private enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT = 1,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		public static void Trigger()
		{
			SetConsoleCtrlHandler(Handler, true);
		}

		private static bool Handler(CtrlType signal)
		{
			switch (signal)
			{
				case CtrlType.CTRL_BREAK_EVENT:
				case CtrlType.CTRL_C_EVENT:
				case CtrlType.CTRL_LOGOFF_EVENT:
				case CtrlType.CTRL_SHUTDOWN_EVENT:
				case CtrlType.CTRL_CLOSE_EVENT:
					foreach (ClientWebSocket webSocket in webSockets)
					{
						webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, new()).Wait();
					}
					Environment.Exit(0);
					return false;

				default:
					return false;
			}
		}
	}
}
