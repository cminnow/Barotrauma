﻿using Barotrauma.Networking;
using System;

namespace Barotrauma.Items.Components
{
    partial class Engine : Powered, IServerSerializable, IClientSerializable
    {
        public void ServerWrite(IWriteMessage msg, Client c, object[] extraData = null)
        {
            //force can only be adjusted at 10% intervals -> no need for more accuracy than this
            msg.WriteRangedInteger((int)(targetForce / 10.0f), -10, 10);
        }

        public void ServerRead(ClientNetObject type, IReadMessage msg, Client c)
        {
            float newTargetForce = msg.ReadRangedInteger(-10, 10) * 10.0f;

            if (item.CanClientAccess(c))
            {
                if (Math.Abs(newTargetForce - targetForce) > 0.01f)
                {
                    GameServer.Log(GameServer.CharacterLogName(c.Character) + " set the force of " + item.Name + " to " + (int)(newTargetForce) + " %", ServerLog.MessageType.ItemInteraction);
                }

                targetForce = newTargetForce;
            }

            //notify all clients of the changed state
            item.CreateServerEvent(this);
        }
    }
}
