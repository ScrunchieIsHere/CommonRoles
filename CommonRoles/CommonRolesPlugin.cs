using BepInEx;
using BepInEx.IL2CPP;
using CrowdedRoles;
using CrowdedRoles.Attributes;
using HarmonyLib;
using Reactor;

namespace CommonRoles
{
    [BepInPlugin(Id)]
    [BepInProcess("Among Us.exe")]
    [BepInDependency(ReactorPlugin.Id)]
    [BepInDependency(RoleApiPlugin.Id)]
    public class CommonRolesPlugin : BasePlugin
    {
        private const string Id = "ru.galster.commonroles";

        private Harmony Harmony { get; } = new (Id);

        public override void Load()
        {
            RegisterCustomRoleAttribute.Register(this);
            RegisterCustomGameOverReasonAttribute.Register(this);
            SheriffOptions.RegisterOptions(this);
            
            Harmony.PatchAll();
        }
    }
}
