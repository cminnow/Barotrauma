﻿using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Xml.Linq;

namespace Barotrauma.Items.Components
{
    partial class Projectile : ItemComponent
    {
        public void ClientRead(ServerNetObject type, IReadMessage msg, float sendingTime)
        {
            bool isStuck = msg.ReadBoolean();
            if (isStuck)
            {
                ushort submarineID = msg.ReadUInt16();
                ushort hullID = msg.ReadUInt16();
                Vector2 simPosition = new Vector2(
                    msg.ReadSingle(), 
                    msg.ReadSingle());
                Vector2 axis = new Vector2(
                    msg.ReadSingle(),
                    msg.ReadSingle());
                UInt16 entityID = msg.ReadUInt16();

                Entity entity       = Entity.FindEntityByID(entityID);
                Submarine submarine = Entity.FindEntityByID(submarineID) as Submarine;
                Hull hull           = Entity.FindEntityByID(hullID) as Hull;
                item.Submarine = submarine;
                item.CurrentHull = hull;
                item.body.SetTransform(simPosition, item.body.Rotation);
                if (entity is Character character)
                {
                    byte limbIndex = msg.ReadByte();
                    if (limbIndex >= character.AnimController.Limbs.Length)
                    {
                        DebugConsole.ThrowError($"Failed to read a projectile update from the server. Limb index out of bounds ({limbIndex}, character: {character.ToString()})");
                        return;
                    }
                    if (character.Removed) { return; }
                    var limb = character.AnimController.Limbs[limbIndex];
                    StickToTarget(limb.body.FarseerBody, axis);
                }
                else if (entity is Structure structure)
                {
                    byte bodyIndex = msg.ReadByte();
                    if (bodyIndex == 255) { bodyIndex = 0; }
                    if (bodyIndex >= structure.Bodies.Count)
                    {
                        DebugConsole.ThrowError($"Failed to read a projectile update from the server. Structure body index out of bounds ({bodyIndex}, structure: {structure.ToString()})");
                        return;
                    }
                    var body = structure.Bodies[bodyIndex];
                    StickToTarget(body, axis);
                }
                else if (entity is Item item)
                {
                    if (item.Removed) { return; }
                    StickToTarget(item.body.FarseerBody, axis);
                }
                else if (entity is  Submarine sub)
                {
                    StickToTarget(sub.PhysicsBody.FarseerBody, axis);
                }
                else
                {
                    DebugConsole.ThrowError($"Failed to read a projectile update from the server. Invalid stick target ({entity?.ToString() ?? "null"}, {entityID})");
                }
            }
            else
            {
                Unstick();
            }
        }
    }
}
