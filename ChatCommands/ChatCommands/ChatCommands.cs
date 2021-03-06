﻿using Mono.Cecil;
using ScrollsModLoader.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ChatCommands
{
	public class ChatCommands : BaseMod
	{
		private List<ChatComm> commands = new List<ChatComm>();

		public ChatCommands()
		{
            commands.Add(new Player());
			commands.Add(new Ignore());
			//commands.Add(new RoomComm());
			commands.Add(new Quit());
			commands.Add(new SetResolution());
            commands.Add(new Trade());
            commands.Add(new Help());
		}

		public static string GetName()
		{
			return "ChatCommands";
		}

		public static int GetVersion()
		{
			return 3;
		}

		public static MethodDefinition[] GetHooks(TypeDefinitionCollection scrollsTypes, int version)
		{
			try {
				return new MethodDefinition[] {
					scrollsTypes["ChatRooms"].Methods.GetMethod("ChatMessage", new Type[]{typeof(RoomChatMessageMessage)}),
					scrollsTypes["Communicator"].Methods.GetMethod("sendRequest", new Type[]{typeof(Message)})
				};
			} catch {
				return new MethodDefinition[] { };
			}
		}

		public override bool BeforeInvoke(InvocationInfo info, out object returnValue)
		{
			returnValue = null;

			if (info.targetMethod.Equals("ChatMessage")) // ChatMessage (received) in ChatRooms
			{
				RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];

				return hooks(false, rcmm);
			}
			else if (info.targetMethod.Equals("sendRequest"))
			{
				if (info.arguments[0] is RoomChatMessageMessage)
				{
					RoomChatMessageMessage rcmm = (RoomChatMessageMessage)info.arguments[0];

					return hooks(true, rcmm);
				}
			}
			return false;
		}

		public override void AfterInvoke(InvocationInfo info, ref object returnValue)
		{
			return;
		}

		/**
		 * returns true when the text message is a command for one of the functions
		 */
		private bool hooks(bool sending, RoomChatMessageMessage rcmm)
		{
			bool h = false;
			foreach (ChatComm cc in commands)
			{
				bool hooksSingle = sending ? cc.hooksSend(rcmm) : cc.hooksReceive(rcmm);

				h |= hooksSingle;
			}
			return h;
		}
	}
}
