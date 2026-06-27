using UnityEngine;
using System.Collections.Generic;

public class AshenveilQuestFactory : MonoBehaviour
{
    private static AshenveilQuestFactory _instance;
    private readonly List<QuestData> _builtQuests = new();

    [Header("Asset References")]
    public AshenveilAssetRegistry assets;

    void Awake() => _instance = this;

    void OnDestroy()
    {
        if (_instance == this)
            _instance = null;
    }

    void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[AshenveilQuestFactory] QuestManager not found.");
            return;
        }

        BuildAllQuests();
        InjectIntoQuestManager();
        TryAutoStartAvailableQuests();
    }

    public static void OnOriginApplied()
    {
        if (_instance != null)
            _instance.TryAutoStartAvailableQuests();
    }

    void BuildAllQuests()
    {
        _builtQuests.Clear();
        _builtQuests.Add(BuildQuest1());
        _builtQuests.Add(BuildQuest2());
        _builtQuests.Add(BuildQuest3());
        _builtQuests.Add(BuildQuest4());
        _builtQuests.Add(BuildQuest5());
        _builtQuests.Add(BuildQuest6());
        _builtQuests.Add(BuildQuest7());
        _builtQuests.Add(BuildQuest8());
        _builtQuests.Add(BuildQuest9());
        _builtQuests.Add(BuildQuest10());
        _builtQuests.Add(BuildQuest11());
        _builtQuests.Add(BuildSinnedGuardianQuest1());
        _builtQuests.Add(BuildBoundArchivistQuest1());
        _builtQuests.Add(BuildForeignEchoQuest1());
    }

    void InjectIntoQuestManager()
    {
        var existing = new List<QuestData>(QuestManager.Instance.allQuests ?? new QuestData[0]);

        foreach (var q in _builtQuests)
        {
            bool dup = false;

            foreach (var e in existing)
                if (e.questID == q.questID)
                {
                    dup = true;
                    break;
                }

            if (!dup)
                existing.Add(q);
        }

        QuestManager.Instance.allQuests = existing.ToArray();
        Debug.Log($"[AshenveilQuestFactory] {_builtQuests.Count} quests registered.");
    }

    void TryAutoStartAvailableQuests()
    {
        foreach (var q in _builtQuests)
            if (QuestManager.Instance.CanStartQuest(q))
                QuestManager.Instance.StartQuest(q);
    }

    QuestData Make(string id, string name, string desc, QuestType type, string[] requiredFlags = null, string[] flagsOnStart = null, string[] flagsOnComplete = null, bool hasTimeLimit = false, float timeLimitSeconds = 0f, bool canFail = false)
    {
        var q = ScriptableObject.CreateInstance<QuestData>();
        q.questID = id;
        q.questName = name;
        q.description = desc;
        q.questType = type;
        q.difficulty = QuestDifficulty.Normal;
        q.requiredFlags = requiredFlags ?? new string[0];
        q.flagsToSetOnStart = flagsOnStart ?? new string[0];
        q.flagsToSetOnComplete = flagsOnComplete ?? new string[0];
        q.hasTimeLimit = hasTimeLimit;
        q.timeLimitSeconds = timeLimitSeconds;
        q.canFail = canFail;
        q.trackObjectives = true;
        q.autoCompleteWhenObjectivesComplete = true;
        q.showOnMap = true;
        q.icon = assets != null ? assets.GetQuestIcon(id) : null;
        return q;
    }

    static QuestObjective Interact(string id, string desc, string interactTag = null, bool optional = false) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.InteractWithObject,
        interactObjectTag = string.IsNullOrEmpty(interactTag) ? id : interactTag,
        targetCount = 1,
        isOptional = optional
    };

    static QuestObjective Talk(string id, string desc, string npcTag = null, int talkCount = 1, bool optional = false) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.TalkToNPC,
        npcTag = string.IsNullOrEmpty(npcTag) ? id : npcTag,
        targetCount = talkCount,
        isOptional = optional
    };

    static QuestObjective Collect(string id, string desc, ItemData item, int count, bool consume = false) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.CollectItems,
        targetItem = item,
        itemCount = count,
        consumeItems = consume
    };

    static QuestObjective Kill(string id, string desc, EnemyData enemy, int count) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.KillEnemies,
        targetEnemy = enemy,
        targetCount = count
    };

    static float TwoDaySeconds()
    {
        float phaseDur = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.phaseDuration : 300f;
        return phaseDur * 4f * 2f;
    }

    QuestData BuildQuest1()
    {
        var q = Make("q01_bir_fincan_huzur", "A Cup of Peace", "Maren called for you early in the morning.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.MarenTeaServed });

        q.objectives = new[]
        {
            Collect("q01_obj1", "Pick 1 Fresh Apple from the garden", assets != null ? assets.freshApple : null, 1),
            Collect("q01_obj2", "Take 1 Cinnamon from the kitchen shelf", assets != null ? assets.cinnamon : null, 1),
            Interact("q01_obj3", "Brew the tea at the stove"),
            Interact("q01_obj4", "Take the cup to Maren")
        };

        q.itemRewards = assets != null ? new ItemData[] { assets.marenNecklace } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 50;
        return q;
    }

    QuestData BuildQuest2()
    {
        var q = Make("q02_hala_duman", "Still Smoke", "Maren told you that three houses have been emptied.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.StillSmokeDone });

        q.objectives = new[]
        {
            Collect("q02_obj1", "Search the blacksmith's house (Torn Contract)", assets != null ? assets.tornContract : null, 1),
            Collect("q02_obj2", "Search the baker's house (Torn Contract)", assets != null ? assets.tornContract : null, 1),
            Collect("q02_obj3", "Search the healer's house (Torn Contract)", assets != null ? assets.tornContract : null, 1),
            Kill("q02_obj4", "Kill 2 Shadow Lurkers outside the village", assets != null ? assets.shadowLurker : null, 2),
            Talk("q02_obj5", "Speak with old Corvin")
        };

        q.itemRewards = assets != null ? new ItemData[] { assets.rustedKey, assets.blacksmithApron } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1, 1 };
        q.experienceReward = 80;
        return q;
    }

    QuestData BuildQuest3()
    {
        var q = Make("q03_kirik_muhur", "Broken Seal", "The contract in the healer's house is missing a piece.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.MissingContractFound });

        q.objectives = new[]
        {
            Talk("q03_obj1", "Talk to 3 different NPCs in the village", talkCount: 3),
            Interact("q03_obj2", "Follow the hidden trail in the village square"),
            Interact("q03_obj3", "Find the box behind the barn")
        };

        q.itemRewards = assets != null ? new ItemData[] { assets.missingContract, assets.villageCoat } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1, 1 };
        q.experienceReward = 60;
        return q;
    }

    QuestData BuildQuest4()
    {
        var q = Make("q04_yasli_adamin_itirafi", "The Old Man's Confession", "Corvin is waiting for you in the evening.", QuestType.Main, requiredFlags: new[] { QuestFlags.StillSmokeDone }, flagsOnComplete: new[] { QuestFlags.ElderTruthKnown, QuestFlags.VossContractPlayerAware });

        q.objectives = new[]
        {
            Talk("q04_obj1", "Listen to Corvin"),
            Interact("q04_obj2", "Find the old square fountain"),
            Interact("q04_obj3", "Take the hidden document beneath the fountain")
        };

        q.itemRewards = assets != null ? new[] { assets.vossDiary } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 120;
        return q;
    }

    QuestData BuildQuest5()
    {
        var q = Make("q05_acik_el", "Open Hand", "The first name in the ledger is at the mill. Find and rescue the captive.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown }, flagsOnComplete: new[] { QuestFlags.FirstPrisonerRescued }, hasTimeLimit: true, timeLimitSeconds: TwoDaySeconds(), canFail: true);

        q.objectives = new[]
        {
            Interact("q05_obj1", "Investigate the village mill"),
            Interact("q05_obj2", "Go to the cave the miller spoke of"),
            Kill("q05_obj3", "Kill the 3 Shadow Guards in the cave", assets != null ? assets.shadowGuard : null, 3),
            Interact("q05_obj4", "Rescue the captive in the cave"),
            Interact("q05_obj5", "Take the captive back to the village")
        };

        q.experienceReward = 200;
        return q;
    }

    QuestData BuildQuest6()
    {
        var q = Make("q06_tarif_defteri", "The Recipe Book", "The rescued captive wants to give Maren an old recipe.", QuestType.Side, requiredFlags: new[] { QuestFlags.FirstPrisonerRescued }, flagsOnComplete: new[] { QuestFlags.MarenRecipeGiven });

        q.objectives = new[]
        {
            Collect("q06_obj1", "Find the Recipe Page in the field", assets != null ? assets.recipePage : null, 1),
            Talk("q06_obj2", "Take the recipe to Maren")
        };

        q.itemRewards = assets != null ? new[] { assets.applePie } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 80;
        return q;
    }

    QuestData BuildQuest7()
    {
        var q = Make("q07_corvinin_borcu", "Corvin's Debt", "After finding the missing contract, Corvin called for you.", QuestType.Side, requiredFlags: new[] { QuestFlags.MissingContractFound }, flagsOnComplete: new[] { QuestFlags.CorvinDebtSettled });

        q.objectives = new[]
        {
            Interact("q07_obj1", "Go to the old church ruins"),
            Kill("q07_obj2", "Kill the 3 Cursed Guards in the ruins", assets != null ? assets.cursedGuard : null, 3),
            Interact("q07_obj3", "Open the chest (Rusted Key required)"),
            Collect("q07_obj4", "Bring the document from the chest to Corvin", assets != null ? assets.corvinsDocument : null, 1)
        };

        q.itemRewards = assets != null ? new[] { assets.corvinSeal } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 150;
        return q;
    }

    QuestData BuildQuest8()
    {
        var q = Make("q08_defterdeki_sesler", "Voices in the Ledger", "In the evening, a stranger approached the one carrying Voss's ledger.", QuestType.Side, requiredFlags: new[] { QuestFlags.VossContractPlayerAware }, flagsOnComplete: new[] { QuestFlags.VossWeakPointKnown });

        q.objectives = new[]
        {
            Talk("q08_obj1", "Speak with the mysterious NPC"),
            Interact("q08_obj2", "Find the symbol they described in the village"),
            Interact("q08_obj3", "Press Corvin's Seal onto the symbol", optional: true)
        };

        q.experienceReward = 100;
        return q;
    }

    QuestData BuildQuest9()
    {
        var q = Make("q09_bekleyis", "The Wait", "Wait for Voss at the village entrance at night.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown }, flagsOnComplete: new[] { QuestFlags.Q09VossWarehouseFound });

        q.objectives = new[]
        {
            Interact("q09_obj1", "Wait for Voss at the village entrance"),
            Interact("q09_obj2", "Don't buy anything from Voss's stall", optional: true),
            Interact("q09_obj3", "Follow Voss"),
            Interact("q09_obj4", "Find Voss's hidden warehouse")
        };

        q.experienceReward = 120;
        return q;
    }

    QuestData BuildQuest10()
    {
        var q = Make("q10_acik_hesap", "Open Account", "You've entered Voss's warehouse. Take out the guards, free the captives, and confront Voss.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown, QuestFlags.Q09VossWarehouseFound });

        q.objectives = new[]
        {
            Kill("q10_obj1", "Kill the 3 Shadow Guards in the warehouse", assets != null ? assets.shadowGuard : null, 3),
            Interact("q10_obj2", "Free the captives in the cages"),
            Talk("q10_obj3", "Confront Voss"),
            Kill("q10_obj4", "Kill Voss (boss)", assets != null ? assets.vossBoss : null, 1)
        };

        q.experienceReward = 500;
        return q;
    }

    QuestData BuildQuest11()
    {
        var q = Make("q11_fincan_basinda", "Over a Cup", "In the morning, Maren is waiting at your door.", QuestType.Side, requiredFlags: new[] { QuestFlags.VossDefeatedClean }, flagsOnComplete: new[] { QuestFlags.ScenarioCompleted });

        q.objectives = new[]
        {
            Talk("q11_obj1", "Talk with Maren"),
            Interact("q11_obj2", "Go to the garden together"),
            Interact("q11_obj3", "Drink one last cup of apple tea")
        };

        q.experienceReward = 0;
        q.currencyRewards = new CurrencyReward[0];
        q.itemRewards = new ItemData[0];
        return q;
    }

    QuestData BuildSinnedGuardianQuest1()
    {
        var q = Make(QuestIds.Q_SG01, "The Debt That Breathes", "Three families. Three names on a list. Your hand, your choice. Shadow Garden believes they are still reachable — held somewhere east of Dragsimo.", QuestType.Main, requiredFlags: new[] { QuestFlags.SinnedGuardianStart }, flagsOnComplete: new[] { QuestFlags.ThreeFamiliesLocated, QuestFlags.ShadowGardenRank1 });

        q.objectives = new[]
        {
            Talk("q_sg01_obj1", "Speak with Maren at night"),
            Talk("q_sg01_obj2", "Meet Aslude at the well"),
            Talk("q_sg01_obj3", "Speak with Corvin (optional)", optional: true),
            Interact("q_sg01_obj4", "Learn the village's western edge (2 of 3 locations)"),
            Interact("q_sg01_obj5", "Track Voss from the western road"),
            Talk("q_sg01_obj6", "Speak with Voss"),
            Interact("q_sg01_obj7", "Find the second wall east of Dragsimo"),
            Interact("q_sg01_obj8", "Examine what is behind the second wall"),
            Talk("q_sg01_obj9", "Return to Aslude with the discovery")
        };

        var sgRewards = new List<ItemData>();
        var sgQty = new List<int>();

        if (assets != null && assets.corvinSeal != null)
        {
            sgRewards.Add(assets.corvinSeal);
            sgQty.Add(1);
        }

        if (assets != null && assets.corvinsTestimony != null)
        {
            sgRewards.Add(assets.corvinsTestimony);
            sgQty.Add(1);
        }

        q.itemRewards = sgRewards.ToArray();
        q.itemRewardQuantities = sgQty.ToArray();
        q.experienceReward = 420;
        return q;
    }

    QuestData BuildBoundArchivistQuest1()
    {
        var q = Make(QuestIds.Q_BA01, "The Reversal Clause", "The file Brahma left behind carries the trail of a lost record. Start with Maren — Ashenveil's past is asked of her.", QuestType.Main, requiredFlags: new[] { QuestFlags.BoundArchivistStart }, flagsOnComplete: new[] { QuestFlags.BoundArchivistQuest1Done });

        q.objectives = new[]
        {
            Talk("obj_speak_maren_gate", "Speak with Maren at the gate"),
            Talk("obj_learn_maren_knowledge", "Learn the past in Maren's kitchen"),
            Talk("obj_find_elis", "Find Elis and learn the three locations", optional: true),
            Talk("obj_handle_eow_operative", "Deal with the EoW operative"),
            Talk("obj_speak_voss_day3", "Speak with Voss on day 3"),
            Interact("obj_cross_reference", "Narrow down the record's location by cross-reference")
        };

        q.experienceReward = 380;
        return q;
    }

    QuestData BuildForeignEchoQuest1()
    {
        var q = Make(QuestIds.Q_FE01, "The Originating Record", "The Axios anomaly drew you to Ashenveil. Follow the shadow at Maren's door — no one knows what lies inside the chamber.", QuestType.Main, requiredFlags: new[] { QuestFlags.ForeignEchoStart }, flagsOnComplete: new[] { QuestFlags.ForeignEchoQuest1Done, QuestFlags.ChamberInteriorSeen });

        q.objectives = new[]
        {
            Interact("obj_follow_shadow_maren", "Follow the shadow at Maren's door"),
            Talk("obj_understand_maren_need", "Understand Maren's mission"),
            Talk("obj_speak_mireya", "Speak with Mireya — get the patrol data", optional: true),
            Talk("obj_meet_chico", "Meet with Chico", optional: true),
            Talk("obj_speak_voss_day3", "Speak with Voss on day 3"),
            Interact("obj_identify_axios_anomaly", "Identify the record source on the chamber floor")
        };

        q.itemRewards = assets != null && assets.axiosCrystal != null ? new[] { assets.axiosCrystal } : new ItemData[0];
        q.itemRewardQuantities = assets != null && assets.axiosCrystal != null ? new[] { 1 } : new int[0];
        q.experienceReward = 360;
        return q;
    }
}
