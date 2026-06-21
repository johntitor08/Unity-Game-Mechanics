using UnityEngine;
using System.Collections.Generic;

public static class AshenveilQuestTriggerCatalog
{
    public enum RecommendedComponent
    {
        UIHoverBridge,
        WorldInteract,
        WorldTalk,
        WorldProgress,
        WorldLocation,
        WorldItem,
        CombatTrigger,
        QuestGiver
    }

    public readonly struct Entry
    {
        public readonly string questID;
        public readonly string objectiveID;
        public readonly string tag;
        public readonly string suggestedSceneObjectName;
        public readonly string parentHint;
        public readonly QuestUIHoverBridge.TriggerKind recommendedKind;
        public readonly RecommendedComponent component;
        public readonly int progressAmount;
        public readonly string notes;
        public readonly Vector2 anchoredPosition;
        public readonly Vector2 size;

        public Entry(string questID, string objectiveID, string suggestedSceneObjectName, QuestUIHoverBridge.TriggerKind kind, RecommendedComponent component, string parentHint = "", string tag = null, int progressAmount = 1, string notes = "", Vector2 anchoredPosition = default, Vector2 size = default)
        {
            this.questID = questID;
            this.objectiveID = objectiveID;
            this.suggestedSceneObjectName = suggestedSceneObjectName;
            this.recommendedKind = kind;
            this.component = component;
            this.parentHint = parentHint;
            this.tag = tag ?? objectiveID;
            this.progressAmount = progressAmount;
            this.notes = notes;
            this.anchoredPosition = anchoredPosition;
            this.size = size == default ? new Vector2(60f, 60f) : size;
        }
    }

    public static readonly Entry[] All =
    {
        new(AshenveilQuestIds.Q01, "q01_obj1", "Ashenveil_Garden_ApplePickup", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "MapCanvas / Garden", notes: "WorldItem + freshApple. Mevcut GardenHoleRegion yakını."),
        new(AshenveilQuestIds.Q01, "q01_obj2", "Ashenveil_Kitchen_CinnamonShelf", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "houseIconsPanel / Kitchen", notes: "WorldItem + cinnamon."),
        new(AshenveilQuestIds.Q01, "q01_obj3", "Ashenveil_Kitchen_Stove", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "houseIconsPanel / Kitchen", notes: "QuestUIHoverBridge · catalogObjectiveID = q01_obj3"),
        new(AshenveilQuestIds.Q01, "q01_obj4", "Ashenveil_Maren_TeaHandIn", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Maren NPC veya kapı hover", notes: "İsteğe bağlı DialogueNode. Talk veya Interact (npcTag = q01_obj4)."),
        new(AshenveilQuestIds.Q02, "q02_obj1", "Ashenveil_House_Blacksmith_Search", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "Köy / Demirci evi", notes: "WorldItem tornContract."),
        new(AshenveilQuestIds.Q02, "q02_obj2", "Ashenveil_House_Baker_Search", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "Köy / Fırıncı evi", notes: "WorldItem tornContract."),
        new(AshenveilQuestIds.Q02, "q02_obj3", "Ashenveil_House_Healer_Search", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "Köy / Şifacı evi", notes: "WorldItem tornContract."),
        new(AshenveilQuestIds.Q02, "q02_obj4", "Ashenveil_Outskirts_ShadowLurker", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.CombatTrigger,
            "Harita dışı / combat map", notes: "CombatTrigger shadowLurker ×2."),
        new(AshenveilQuestIds.Q02, "q02_obj5", "Ashenveil_NPC_Corvin", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Köy meydanı", notes: "QuestUIHoverBridge + Corvin diyalog. npcTag = q02_obj5"),
        new(AshenveilQuestIds.Q03, "q03_obj1", "Ashenveil_Villager_Talk_01", QuestUIHoverBridge.TriggerKind.DirectProgress, RecommendedComponent.UIHoverBridge,
            "Köy", tag: "q03_obj1", notes: "3 ayrı NPC: Villager_Talk_02, _03. QuestUIHoverBridge kind=DirectProgress."),
        new(AshenveilQuestIds.Q03, "q03_obj1", "Ashenveil_Villager_Talk_02", QuestUIHoverBridge.TriggerKind.DirectProgress, RecommendedComponent.UIHoverBridge,
            "Köy", tag: "q03_obj1"),
        new(AshenveilQuestIds.Q03, "q03_obj1", "Ashenveil_Villager_Talk_03", QuestUIHoverBridge.TriggerKind.DirectProgress, RecommendedComponent.UIHoverBridge,
            "Köy", tag: "q03_obj1"),
        new(AshenveilQuestIds.Q03, "q03_obj2", "Ashenveil_Square_HiddenTrail", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "townIcon / meydan"),
        new(AshenveilQuestIds.Q03, "q03_obj3", "Ashenveil_Barn_BackChest", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Ahır bölgesi"),
        new(AshenveilQuestIds.Q04, "q04_obj1", "Ashenveil_NPC_Corvin_Evening", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Corvin", notes: "Akşam diyalogu."),
        new(AshenveilQuestIds.Q04, "q04_obj2", "Ashenveil_OldSquare_Fountain", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Eski meydan"),
        new(AshenveilQuestIds.Q04, "q04_obj3", "Ashenveil_Fountain_HiddenDocument", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Çeşme altı", notes: "Tamamlanınca vossDiary envantere (ayrı script veya WorldItem)."),
        new(AshenveilQuestIds.Q05, "q05_obj1", "Ashenveil_Mill_Investigate", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Değirmen"),
        new(AshenveilQuestIds.Q05, "q05_obj2", "Ashenveil_Mill_CaveEntrance", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Değirmen / mağara girişi"),
        new(AshenveilQuestIds.Q05, "q05_obj3", "Ashenveil_Cave_ShadowGuards", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.CombatTrigger,
            "Mağara", notes: "shadowGuard ×3."),
        new(AshenveilQuestIds.Q05, "q05_obj4", "Ashenveil_Cave_PrisonerCage", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Mağara içi"),
        new(AshenveilQuestIds.Q05, "q05_obj5", "Ashenveil_Village_ReturnPrisoner", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Köy girişi"),
        new(AshenveilQuestIds.Q06, "q06_obj1", "Ashenveil_Field_RecipePage", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.WorldItem,
            "Tarla", notes: "WorldItem recipePage."),
        new(AshenveilQuestIds.Q06, "q06_obj2", "Ashenveil_Maren_RecipeDelivery", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Maren"),
        new(AshenveilQuestIds.Q07, "q07_obj1", "Ashenveil_ChurchRuins_Entrance", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "churchIcon / harabe"),
        new(AshenveilQuestIds.Q07, "q07_obj2", "Ashenveil_ChurchRuins_CursedGuards", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.CombatTrigger,
            "Harabe içi", notes: "cursedGuard ×4."),
        new(AshenveilQuestIds.Q07, "q07_obj3", "Ashenveil_ChurchRuins_Vault", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Harabe", notes: "rustedKey kontrolü ayrı script."),
        new(AshenveilQuestIds.Q07, "q07_obj4", "Ashenveil_Corvin_DocumentReturn", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Corvin", notes: "Collect corvinsDocument — otomatik envanter."),
        new(AshenveilQuestIds.Q08, "q08_obj1", "Ashenveil_NPC_MysteriousStranger", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Köy / gece"),
        new(AshenveilQuestIds.Q08, "q08_obj2", "Ashenveil_Village_Symbol", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Köy"),
        new(AshenveilQuestIds.Q08, "q08_obj3", "Ashenveil_Village_Symbol_Seal", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Sembol", notes: "Opsiyonel. corvinSeal kontrolü."),
        new(AshenveilQuestIds.Q09, "q09_obj1", "Ashenveil_Gate_VossWait", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Köy girişi / gece"),
        new(AshenveilQuestIds.Q09, "q09_obj2", "Ashenveil_Voss_Stall", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Voss tezgahı", notes: "Opsiyonel."),
        new(AshenveilQuestIds.Q09, "q09_obj3", "Ashenveil_Gate_FollowVoss", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Köy girişi"),
        new(AshenveilQuestIds.Q09, "q09_obj4", "Ashenveil_Voss_HiddenWarehouse", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Depo girişi"),
        new(AshenveilQuestIds.Q10, "q10_obj1", "Ashenveil_Warehouse_ShadowGuards", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.CombatTrigger,
            "Depo", notes: "shadowGuard ×3."),
        new(AshenveilQuestIds.Q10, "q10_obj2", "Ashenveil_Warehouse_PrisonCages", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Depo"),
        new(AshenveilQuestIds.Q10, "q10_obj3", "Ashenveil_Warehouse_VossConfront", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Depo", notes: "Sonra CombatTrigger vossBoss (q10_obj4)."),
        new(AshenveilQuestIds.Q10, "q10_obj4", "Ashenveil_Warehouse_VossBoss", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.CombatTrigger,
            "Depo", notes: "AshenveilVossWeakPoint sahnesinde."),
        new(AshenveilQuestIds.Q11, "q11_obj1", "Ashenveil_Maren_FinaleTalk", QuestUIHoverBridge.TriggerKind.Talk, RecommendedComponent.UIHoverBridge,
            "Maren kapısı"),
        new(AshenveilQuestIds.Q11, "q11_obj2", "Ashenveil_Garden_FinaleWalk", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Bahçe", notes: "Mevcut gardenIcon / GardenHoleRegion bölgesi."),
        new(AshenveilQuestIds.Q11, "q11_obj3", "Ashenveil_Garden_FinaleTea", QuestUIHoverBridge.TriggerKind.Interact, RecommendedComponent.UIHoverBridge,
            "Bahçe / çay"),
    };

    static readonly Dictionary<string, Entry> ByObjectiveId = BuildLookup();

    static Dictionary<string, Entry> BuildLookup()
    {
        var map = new Dictionary<string, Entry>();

        foreach (var entry in All)
        {
            if (!map.ContainsKey(entry.objectiveID))
                map.Add(entry.objectiveID, entry);
        }

        return map;
    }

    public static bool TryGet(string objectiveID, out Entry entry) => ByObjectiveId.TryGetValue(objectiveID, out entry);

    public static string BackgroundFor(string sceneObjectName)
    {
        if (string.IsNullOrEmpty(sceneObjectName))
            return "";

        string n = sceneObjectName;

        if (n.Contains("Garden"))
            return "bg_apple_garden";

        if (n.Contains("Kitchen") || n.Contains("Maren"))
            return "bg_maren_kitchen";

        if (n.Contains("House_Blacksmith"))
            return "bg_blacksmith_home";

        if (n.Contains("House_Baker"))
            return "bg_baker_home";

        if (n.Contains("House_Healer"))
            return "bg_healer_home";

        if (n.Contains("Outskirts") || n.Contains("Field"))
            return "bg_village_fields";

        if (n.Contains("Barn"))
            return "bg_barn";

        if (n.Contains("Mill"))
            return "bg_village_mill";

        if (n.Contains("Cave"))
            return "bg_cave";

        if (n.Contains("ChurchRuins"))
            return "bg_church_ruins";

        if (n.Contains("Warehouse"))
            return "bg_voss_warehouse";

        if (n.Contains("Voss_Stall"))
            return "bg_voss_stall";

        if (n.Contains("Gate") || n.Contains("ReturnPrisoner"))
            return "bg_village_gate";

        if (n.Contains("Corvin") || n.Contains("Villager") || n.Contains("Square") || n.Contains("Fountain") || n.Contains("Symbol") || n.Contains("Mysterious"))
            return "bg_village_square";

        return "";
    }

    public static IEnumerable<Entry> GetForQuest(string questID)
    {
        foreach (var entry in All)
        {
            if (entry.questID == questID)
                yield return entry;
        }
    }

    public static string FormatSetupReport()
    {
        var sb = new System.Text.StringBuilder();
        string currentQuest = null;

        foreach (var e in All)
        {
            if (e.questID != currentQuest)
            {
                currentQuest = e.questID;
                sb.AppendLine($"## {currentQuest}");
            }

            sb.AppendLine($"[{e.objectiveID}] {e.suggestedSceneObjectName}");
            sb.AppendLine($"Kind: {e.recommendedKind} | Component: {e.component} | Parent: {e.parentHint}");

            if (!string.IsNullOrEmpty(e.notes))
                sb.AppendLine($"Note: {e.notes}");
        }

        return sb.ToString();
    }
}
