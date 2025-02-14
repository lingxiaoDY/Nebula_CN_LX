﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Nebula.Patches;

namespace Nebula.Roles.ImpostorRoles
{
    public class EvilAce : Role
    {
        private Module.CustomOption killCoolDownMultiplierOption;
        private Module.CustomOption canKnowDeadNonImpostorsRolesOption;
        private Module.CustomOption canKnowRolesOnlyMyMurdersOption;

        public override void LoadOptionData()
        {
            killCoolDownMultiplierOption = CreateOption(Color.white, "killCoolDown", 0.5f, 0.125f, 1f, 0.125f);
            killCoolDownMultiplierOption.suffix = "cross";
            canKnowDeadNonImpostorsRolesOption = CreateOption(Color.white, "canKnowDeadNonImpostorsRoles", false);
            canKnowRolesOnlyMyMurdersOption = CreateOption(Color.white, "canKnowRolesOnlyMyMurders", true).AddPrerequisite(canKnowDeadNonImpostorsRolesOption);
        }

        public override void OnAnyoneDied(byte playerId)
        {
            try
            {
                PlayerControl p = Helpers.playerById(playerId);
                var data = p.GetModData();
                //赤文字は何もしない
                if (data.role.category == RoleCategory.Impostor || data.role == Roles.Spy) return;
                if((!canKnowRolesOnlyMyMurdersOption.getBool()) || Game.GameData.data.deadPlayers[p.PlayerId].MurderId==PlayerControl.LocalPlayer.PlayerId)
                data.RoleInfo= Helpers.cs(data.role.Color, Language.Language.GetString("role." + data.role.LocalizeName + ".name"));
            }
            catch { }
        }

        public override void OnRoleRelationSetting()
        {
            RelatedRoles.Add(Roles.Jackal);
        }

        public EvilAce()
                : base("EvilAce", "evilAce", Palette.ImpostorRed, RoleCategory.Impostor, Side.Impostor, Side.Impostor,
                     Impostor.impostorSideSet, Impostor.impostorSideSet, Impostor.impostorEndSet,
                     true, VentPermission.CanUseUnlimittedVent, true, true, true)
        {

        }

        public override void SetKillCoolDown(ref float multiplier, ref float addition) {
            int impostorSide = 0;
            foreach(Game.PlayerData data in Game.GameData.data.players.Values)
            {
                if (!data.IsAlive)
                {
                    continue;
                }
                if (data.role.side == Side.Impostor)
                {
                    impostorSide++;
                }
            }
            if (impostorSide == 1)
            {
                multiplier = killCoolDownMultiplierOption.getFloat();
            }
        }
    }
}
