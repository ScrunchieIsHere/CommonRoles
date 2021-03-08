using BepInEx.IL2CPP;
using CrowdedRoles.Attributes;
using CrowdedRoles.Extensions;
using CrowdedRoles.Options;
using CrowdedRoles.Roles;
using UnityEngine;

namespace CommonRoles
{
    [RegisterCustomRole]
    public class SheriffRole : BaseRole
    {
        public SheriffRole(BasePlugin plugin) : base(plugin)
        {
        }

        public override string Name { get; } = "Sheriff";
        public override Color Color { get; } = new Color(1f, 165f / 255, 0, 1);
        public override string Description { get; } = "Kill the [FF0000FF]Impostor";
        public override Team Team { get; } = Team.Crewmate;

        public override bool CanKill(PlayerControl? target)
            => target == null || // if target is null api asks you, can this person kill in general (to enable kill button e.g.)
               !target.Data.IsDead && 
               !target.AmOwner && 
               target.Visible; // support mods like invisibility

        // PreKill stuff is gonna be reworked most likely
        public override bool PreKill(ref PlayerControl killer, ref PlayerControl target, ref CustomMurderOptions options)
        {
            if (target != killer &&  target.GetTeam() != Team.Impostor)
            // if target is not impostor-sided
            {
                if (SheriffOptions.SheriffsTargetDies.Value)
                {
                    options |= CustomMurderOptions.Force | CustomMurderOptions.NoSnap;
                    killer.RpcCustomMurderPlayer(target, CustomMurderOptions.Force | CustomMurderOptions.NoSnap); // kill target first so they can't spam-report you
                }
                target = killer;
            }

            return true;
        }
    }

    public static class SheriffOptions
    {
        public static CustomToggleOption SheriffsTargetDies { get; } = new ("Sheriff's target dies");

        public static void RegisterOptions(BasePlugin plugin)
        {
            SheriffsTargetDies.Register(plugin);
        }
    }
}