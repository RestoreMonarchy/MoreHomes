﻿using HarmonyLib;
using SDG.Unturned;
using Steamworks;
using System;
using RestoreMonarchy.MoreHomes.Helpers;
using RestoreMonarchy.MoreHomes.Models;
using Rocket.Unturned.Chat;

namespace RestoreMonarchy.MoreHomes.Patches
{
	[HarmonyPatch(typeof(BarricadeManager), "ReceiveClaimBedRequest")]
    class BarricadeManager_AskClaimBed_Patch
    {
        [HarmonyPrefix]
        static bool ReceiveClaimBedRequest_Prefix(in ServerInvocationContext context, byte x, byte y, ushort plant, ushort index)
        {
			BarricadeRegion barricadeRegion;
			if (BarricadeManager.tryGetRegion(x, y, plant, out barricadeRegion))
			{
				Player player = context.GetPlayer();
				CSteamID steamID = player.channel.owner.playerID.steamID;

				if (player == null)
				{
					return false;
				}
				if (player.life.isDead)
				{
					return false;
				}
				if ((int)index >= barricadeRegion.drops.Count)
				{
					return false;
				}
				if ((barricadeRegion.drops[(int)index].model.transform.position - player.transform.position).sqrMagnitude > 400f)
				{
					return false;
				}

				InteractableBed interactableBed = barricadeRegion.drops[(int)index].interactable as InteractableBed;
				if (interactableBed != null && interactableBed.isClaimable && interactableBed.checkClaim(player.channel.owner.playerID.steamID))
				{
					if (interactableBed.isClaimed)
					{
						var home = HomesHelper.GetPlayerHome(steamID, interactableBed);
						HomesHelper.RemoveHome(steamID, home);
						home.Unclaim();
					}
					else
					{
						var playerData = HomesHelper.GetOrCreatePlayer(steamID);
						int maxHomes = VipHelper.GetPlayerMaxHomes(steamID.ToString());
						if (maxHomes == 1 && playerData.Homes.Count == 1)
						{
							foreach (var home in playerData.Homes.ToArray())
							{
								HomesHelper.RemoveHome(steamID, home);
								home.Unclaim();
							}
						}
						else if (maxHomes <= playerData.Homes.Count)
						{
							UnturnedChat.Say(steamID, MoreHomesPlugin.Instance.Translate("MaxHomesWarn"), MoreHomesPlugin.Instance.MessageColor);
							return false;
						}

						var playerHome = new PlayerHome(playerData.GetUniqueHomeName(), interactableBed);
						playerData.Homes.Add(playerHome);
						playerHome.Claim(player);
						UnturnedChat.Say(steamID, MoreHomesPlugin.Instance.Translate("HomeClaimed", playerHome.Name), MoreHomesPlugin.Instance.MessageColor);
					}
				}
			}

			return false;
		}
    }
}
