﻿using common;
using wServer.core;
using wServer.core.objects;
using wServer.networking;
using wServer.networking.packets.outgoing;

namespace wServer.core.net.handlers
{
    public class PlayerShootHandler : IMessageHandler
    {
        public int Time { get; set; }
        public byte BulletId { get; set; }
        public int ContainerType { get; set; }
        public Position StartingPos { get; set; }
        public float Angle { get; set; }

        public override MessageId MessageId => MessageId.PLAYERSHOOT;

        public override void Handle(Client client, NReader rdr, ref TickTime tickTime)
        {
            Time = rdr.ReadInt32();
            BulletId = rdr.ReadByte();
            ContainerType = rdr.ReadInt32();
            StartingPos = Position.Read(rdr);
            Angle = rdr.ReadSingle();

            var player = client.Player;

            if (player.World.DisableShooting)
            {
                client.Disconnect("Attempting to shoot in a disabled world");
                return;
            }

            if (player.Inventory[0] == null || player.Inventory[1] == null || !player.GameServer.Resources.GameData.Items.TryGetValue((ushort)ContainerType, out var item))
            {
                client.Disconnect("Attempting to shoot a invalid item");
                return;
            }

            if (item.ObjectType == player.Inventory[1].ObjectType)
            {
                if (player.World.DisableAbilities)
                    client.Disconnect("Attempting to activate ability in a disabled world");
                return; // ability shoot handled by useitem
            }

            // validate
            var result = player.ValidatePlayerShoot(item, Time);
            if (result == PlayerShootStatus.ITEM_MISMATCH)
            {
                System.Console.WriteLine($"PlayerShoot validation ITEM_MISMATCH ({player.Name}:{player.AccountId}): {item.ObjectType} != {player.Inventory[0].ObjectType}");
                return;
            }

            // create projectile and show other players
            var prjDesc = item.Projectiles[0]; //Assume only one
            var prj = player.PlayerShootProjectile(BulletId, prjDesc, item.ObjectType, Time, StartingPos, Angle);

            player.World.AddProjectile(prj);

            player.World.BroadcastIfVisibleExclude(new AllyShoot()
            {
                OwnerId = player.Id,
                Angle = Angle,
                ContainerType = ContainerType,
                BulletId = BulletId
            }, player, player);
            player.FameCounter.Shoot(prj);
        }
    }
}
