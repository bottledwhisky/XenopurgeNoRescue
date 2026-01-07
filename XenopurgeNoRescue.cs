using HarmonyLib;
using MelonLoader;
using SpaceCommander;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WorldMap;
using static WorldMap.Enumerations;
[assembly: MelonInfo(typeof(XenopurgeNoRescue.XenopurgeNoRescue), "Xenopurge No Rescue", "1.0.0", "Felix Hao")]
[assembly: MelonGame("Traptics", "Xenopurge")]
namespace XenopurgeNoRescue
{
    public class XenopurgeNoRescue : MelonMod
    {
        public static MelonPreferences_Category category;
        public static MelonPreferences_Entry<bool> enabled;
        public override void OnInitializeMelon()
        {
            LoggerInstance.Msg("Xenopurge No Rescue Loaded!");
        }
    }
    [HarmonyPatch]
    public class GenerateLocationsPatch
    {
        // Patch GenerateLocations instead of SetUpBattleNodes
        static MethodBase TargetMethod()
        {
            var assembly = Assembly.GetAssembly(typeof(WorldMapConfigSO));
            var type = assembly.GetType("WorldMap.WorldMapLocationsGenerator");
            var method = AccessTools.Method(type, "GenerateLocations");

            return method;
        }
        [HarmonyPostfix]
        public static void Postfix(object __instance, WorldMapNode[][] grid)
        {

            var instanceType = __instance.GetType();
            var configField = AccessTools.Field(instanceType, "_worldMapConfigSO");
            var worldMapConfig = (WorldMapConfigSO)configField.GetValue(__instance);

            var randomField = AccessTools.Field(instanceType, "_random");
            var random = (Random)randomField.GetValue(__instance);

            // Get the current squad unit count
            int initialSquadSize = GetInitialSquadSize();

            // Get WorldMapNode type
            var assembly = Assembly.GetAssembly(typeof(WorldMapConfigSO));
            var worldMapNodeType = assembly.GetType("WorldMap.WorldMapNode");
            var typeProperty = AccessTools.Property(worldMapNodeType, "Type");
            var missionField = AccessTools.Field(worldMapNodeType, "_battleMission");
            // Track RecruitUpgrade nodes encountered
            int recruitUpgradeCount = 0;
            bool hasMaxUnits = false;
            int totalNodes = 0;
            int battleNodes = 0;
            int rescueMissions = 0;
            int replacedMissions = 0;
            // Iterate through floors in order
            for (int floor = 0; floor < grid.Length; floor++)
            {
                for (int col = 0; col < grid[floor].Length; col++)
                {
                    WorldMapNode node = grid[floor][col];
                    if (node == null)
                    {
                        continue;
                    }
                    totalNodes++;
                    // Get node type
                    var nodeType = node.Type;
                    // Count RecruitUpgrade nodes
                    if (nodeType == WorldMapNodeType.RecruitUpgrade)
                    {
                        recruitUpgradeCount++;

                        // Check if we'll have 4 or more units after this recruit
                        if (initialSquadSize + recruitUpgradeCount >= 4)
                        {
                            hasMaxUnits = true;

                        }
                    }
                    // Check if this is a Battle or EliteBattle node
                    if (nodeType == WorldMapNodeType.Battle || nodeType == WorldMapNodeType.EliteBattle)
                    {
                        battleNodes++;
                        // Check if this node has a rescueUnit mission
                        if (missionField != null)
                        {
                            var currentMission = (SpaceCommander.Enumerations.BattleMission)missionField.GetValue(node);

                            if (currentMission == SpaceCommander.Enumerations.BattleMission.rescueUnit)
                            {
                                rescueMissions++;
                                if (hasMaxUnits)
                                {
                                    // Re-randomize to a non-rescueUnit mission
                                    int missionCount = Enum.GetValues(typeof(SpaceCommander.Enumerations.BattleMission)).Length;
                                    int randomIndex = random.Next(0, missionCount - 1);
                                    // If we hit rescueUnit's index, use the last index instead
                                    SpaceCommander.Enumerations.BattleMission newMission =
                                        (SpaceCommander.Enumerations.BattleMission)
                                        (randomIndex >= (int)SpaceCommander.Enumerations.BattleMission.rescueUnit
                                            ? randomIndex + 1
                                            : randomIndex);

                                    // Use reflection to call SetBattleMission or set the field directly
                                    var setBattleMissionMethod = AccessTools.Method(worldMapNodeType, "SetBattleMission");
                                    if (setBattleMissionMethod != null)
                                    {
                                        setBattleMissionMethod.Invoke(node, new object[] { newMission });

                                    }
                                    else
                                    {
                                        // Fallback: set field directly if method not found
                                        missionField.SetValue(node, newMission);

                                    }
                                    replacedMissions++;
                                }
                            }
                        }
                        else
                        {
                            MelonLogger.Warning($"  Floor {floor}, Col {col}: Could not find _battleMission field!");
                        }
                    }
                }
            }
        }
        private static int GetInitialSquadSize()
        {
            try
            {
                // Find InspectSquadDetails_SquadsSelectionDirectory instance
                var type = AccessTools.TypeByName("InspectSquadDetails_SquadsSelectionDirectory");
                if (type == null)
                {
                    MelonLogger.Warning("Could not find InspectSquadDetails_SquadsSelectionDirectory type, defaulting to 3 units");
                    return 3;
                }
                // Try to find an instance of this type
                var instance = UnityEngine.Object.FindFirstObjectByType(type);
                if (instance == null)
                {
                    MelonLogger.Warning("Could not find InspectSquadDetails_SquadsSelectionDirectory instance, defaulting to 3 units");
                    return 3;
                }
                // Get the _squadData field
                var squadDataField = AccessTools.Field(type, "_squadData");
                if (squadDataField == null)
                {
                    MelonLogger.Warning("Could not find _squadData field, defaulting to 3 units");
                    return 3;
                }
                var squadData = squadDataField.GetValue(instance) as SquadData;
                if (squadData == null)
                {
                    MelonLogger.Warning("SquadData is null, defaulting to 3 units");
                    return 3;
                }
                int unitCount = squadData.SquadUnitsCount;

                return unitCount;
            }
            catch (Exception ex)
            {
                MelonLogger.Error($"Error getting initial squad size: {ex.Message}");
                return 3;
            }
        }
    }
}
