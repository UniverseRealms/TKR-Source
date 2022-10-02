﻿using TKR.Shared;
using TKR.WorldServer.core.miscfile.thread;
using TKR.WorldServer.core.net.handlers;
using TKR.WorldServer.core.objects;

namespace TKR.WorldServer.core.commands
{
    public abstract partial class Command
    {
        internal class ClearSpawns : Command
        {
            public override RankingType RankRequirement => RankingType.Admin;
            public override string CommandName => "clearspawn";
            public override string Alias => "cs";

            protected override bool Process(Player player, TickTime time, string args)
            {
                var f = player.GameServer.WorldManager.GetWorlds();
                foreach(var world in f)
                    world.ForeachPlayer(_ =>
                    {
                        for (var i = 0; i < 20; i++)
                        {
                            if (_.Inventory[i]?.ObjectId == "Crown")
                                _.Inventory[i] = null;
                        }
                    });

                var total = 0;
                foreach (var entity in player.World.Enemies.Values)
                    if (entity.Spawned)
                    {
                        entity.Death(ref time);
                        total++;
                    }

                foreach (var entity in player.World.StaticObjects.Values)
                    if (entity.Spawned)
                    {
                        entity.World.LeaveWorld(entity);
                        total++;
                    }

                player.SendInfo($"{total} spawned entit{(total > 1 ? "ies" : "y")} removed!");
                return true;
            }
        }
    }
}
