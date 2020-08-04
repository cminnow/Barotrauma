﻿using Barotrauma.Networking;
using System;

namespace Barotrauma.Items.Components
{
    partial class Projectile : ItemComponent
    {
        public void ServerWrite(IWriteMessage msg, Client c, object[] extraData = null)
        {
            bool stuck = StickTarget != null && !item.Removed && !StickTargetRemoved();
            msg.Write(stuck);
            if (stuck)
            {
                msg.Write(item.Submarine?.ID ?? Entity.NullEntityID);
                msg.Write(item.CurrentHull?.ID ?? Entity.NullEntityID);
                msg.Write(item.SimPosition.X);
                msg.Write(item.SimPosition.Y);
                msg.Write(stickJoint.Axis.X);
                msg.Write(stickJoint.Axis.Y);
                if (StickTarget.UserData is Structure structure)
                {
                    msg.Write(structure.ID);
                    int bodyIndex = structure.Bodies.IndexOf(StickTarget);
                    msg.Write((byte)(bodyIndex == -1 ? 0 : bodyIndex));
                }
                else if (StickTarget.UserData is Entity entity)
                {
                    msg.Write(entity.ID);
                }
                else if (StickTarget.UserData is Limb limb)
                {
                    msg.Write(limb.character.ID);
                    msg.Write((byte)Array.IndexOf(limb.character.AnimController.Limbs, limb));
                }
                else
                {
                    throw new NotImplementedException(StickTarget.UserData?.ToString() ?? "null" + " is not a valid projectile stick target.");
                }
            }
        }
    }
}
