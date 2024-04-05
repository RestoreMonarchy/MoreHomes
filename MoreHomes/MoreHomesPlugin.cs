﻿using HarmonyLib;
using RestoreMonarchy.MoreHomes.Components;
using RestoreMonarchy.MoreHomes.Services;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.MoreHomes
{
    public class MoreHomesPlugin : RocketPlugin<MoreHomesConfiguration>
    {        
        public static MoreHomesPlugin Instance { get; private set; }
        public IRocketPlugin TeleportationPlugin { get; private set; }

        public Dictionary<string, DateTime> PlayerCooldowns { get; set; }

        public DataService DataService { get; private set; }

        public MovementDetectorComponent MovementDetector { get; set; }

        public Color MessageColor { get; set; }

        public const string HarmonyInstanceId = "com.restoremonarchy.morehomes";
        private Harmony HarmonyInstance;

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);

            PlayerCooldowns = new Dictionary<string, DateTime>();
            
            HarmonyInstance = new Harmony(HarmonyInstanceId);
            HarmonyInstance.PatchAll(Assembly);

            DataService = gameObject.AddComponent<DataService>();

            MovementDetector = gameObject.AddComponent<MovementDetectorComponent>();

            R.Plugins.OnPluginsLoaded += OnPluginsLoaded;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log("Brought to you by RestoreMonarchy.com", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            HarmonyInstance?.UnpatchAll(HarmonyInstanceId);
            HarmonyInstance = null;

            Destroy(DataService);

            R.Plugins.OnPluginsLoaded -= OnPluginsLoaded;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        private void OnPluginsLoaded()
        {
            IRocketPlugin plugin = R.Plugins.GetPlugin("Teleportation");
            if (plugin != null)
            {   
                TeleportationPlugin = plugin;
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList(){

            { "HomeCooldown", "You have to wait {0} seconds to use home again" },
            { "HomeDelayWarn", "You will be teleported to your home in {0} seconds" },
            { "MaxHomesWarn", "You cannot have more homes" },
            { "BedDestroyed", "Your home got destroyed or unclaimed! Teleportation canceled" },
            { "WhileDriving", "You cannot teleport while driving" },
            { "NoHome", "You don't have any home to teleport or name doesn't match any" },
            { "HomeSuccess", "Successfully teleported You to your {0} home!" },
            { "HomeList", "Your homes [{0}/{1}]: " },
            { "NoHomes", "You don't have any home" },
            { "DestroyHomeFormat", "Format: /destroyhome <name>" },
            { "HomeNotFound", "No home match {0} name" },
            { "DestroyHomeSuccess", "Successfully destroyed your home {0}!" },
            { "RenameHomeFormat", "Format: /renamehome <name> <rename>" },
            { "HomeAlreadyExists", "You already have a home named {0}" },
            { "RenameHomeSuccess", "Successfully renamed home {0} to {1}!" },
            { "WhileRaid", "You can't teleport while in raiding" },
            { "WhileCombat", "You can't teleport while in combat" },
            { "RestoreHomesSuccess", "Successfully restored {0} homes!" },
            { "RemoveHome", "Your {0} home got removed!" },
            { "RenameHomeSuccess", "Successfully renamed home {0} to {1}!" },
            { "HomeClaimed", "Your new claimed home name is {0}" },
            { "HomeTeleportationFailed", "Failed to teleport you to {0} home" },
            { "HomeDestroyed", "Your home {0} got destroyed or you salvaged it!" },
            { "HomeCanceledYouMoved", "Your home teleportation was canceled because you moved" }
        };        
    }
}
