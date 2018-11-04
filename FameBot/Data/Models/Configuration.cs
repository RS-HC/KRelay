﻿using Lib_K_Relay.Networking.Packets.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FameBot.Data.Models
{
    public class Configuration
    {
        public float AutonexusThreshold { get; set; }
        public int TickCountThreshold { get; set; }
        public bool EscapeIfNoTargets { get; set; }
        public float TeleportDistanceThreshold { get; set; }
        public float FollowDistanceThreshold { get; set; }
        public bool AutoConnect { get; set; }
        public bool EnableEnemyAvoidance { get; set; }
        public float EnemyAvoidanceDistance { get; set; }
        public float TrainTargetPercentage { get; set; }

        public bool FindClustersNearCenter { get; set; }
        public float Epsilon { get; set; }
        public int MinPoints { get; set; }

        public Location FountainLocation { get; set; }
        public Location RealmLocation { get; set; }

        public string FlashPlayerName { get; set; }

        public Configuration()
        {
            AutonexusThreshold = 45f;
            TickCountThreshold = 10;
            TeleportDistanceThreshold = 15f;
            FollowDistanceThreshold = 1.5f;
            AutoConnect = true;
            EnableEnemyAvoidance = true;
            EnemyAvoidanceDistance = 6f;
            TrainTargetPercentage = 30f;
            FindClustersNearCenter = true;
            Epsilon = 8f;
            MinPoints = 5;
            EscapeIfNoTargets = true;
            RealmLocation = new Location(134f, 112f);
            FountainLocation = new Location(134f, 134f);
            FlashPlayerName = "flash";
        }
    }
}