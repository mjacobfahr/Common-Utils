using CommonUtils.Core.Interfaces;
using Exiled.API.Enums;
using System.Collections.Generic;
using UnityEngine;

namespace CommonUtils.Config.ConfigObjects;

public class Scp914TeleportChance : IChanceObjectD
{
    public ZoneType Zone { get; set; } = ZoneType.Unspecified;

    public List<RoomType> IgnoredRooms { get; set; } = new();

    public RoomType Room { get; set; } = RoomType.Unknown;

    public Vector3 Offset { get; set; } = Vector3.zero;

    public double Chance { get; set; }

    public float Damage { get; set; } = 0f;

    public void Deconstruct(out RoomType room, out List<RoomType> ignoredRooms, out Vector3 offset, out double chance, out float damage, out ZoneType zone)
    {
        room = Room;
        ignoredRooms = IgnoredRooms;
        offset = Offset;
        chance = Chance;
        damage = Damage;
        zone = Zone;
    }
}