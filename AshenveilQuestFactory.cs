using UnityEngine;
using System.Collections.Generic;

public class AshenveilQuestFactory : MonoBehaviour
{
    private static AshenveilQuestFactory _instance;
    private readonly List<QuestData> _builtQuests = new();

    [Header("Asset References")]
    public AshenveilAssetRegistry assets;

    void Awake() => _instance = this;

    void Start()
    {
        if (QuestManager.Instance == null)
        {
            Debug.LogError("[AshenveilQuestFactory] QuestManager not found.");
            return;
        }

        BuildAllQuests();
        InjectIntoQuestManager();
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

    static QuestData Make(string id, string name, string desc, QuestType type, string[] requiredFlags = null, string[] flagsOnStart = null, string[] flagsOnComplete = null, bool hasTimeLimit = false, float timeLimitSeconds = 0f, bool canFail = false)
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
        q.showOnMap = true;
        return q;
    }

    static QuestObjective Interact(string id, string desc, bool optional = false) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.InteractWithObject,
        targetCount = 1,
        isOptional = optional
    };

    static QuestObjective Talk(string id, string desc, bool optional = false) => new()
    {
        objectiveID = id,
        description = desc,
        type = QuestObjectiveType.TalkToNPC,
        targetCount = 1,
        isOptional = optional
    };

    static QuestObjective Collect(string id, string desc, ItemData item, int count, bool consume = true) => new()
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
        var q = Make("q01_bir_fincan_huzur", "Bir Fincan Huzur", "Maren seni sabah erkenden çağırdı.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.MarenTeaServed });

        q.objectives = new[]
        {
            Collect("q01_obj1", "Bahçeden 3 Taze Elma topla", assets != null ? assets.freshApple : null, 3),
            Collect("q01_obj2", "Mutfak rafından 1 Tarçın al", assets != null ? assets.cinnamon : null, 1),
            Interact("q01_obj3", "Ocakta çayı demle"),
            Interact("q01_obj4", "Fincanı Maren'e götür")
        };

        q.itemRewards = assets != null ? new[] { assets.appleTea } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 50;
        return q;
    }

    QuestData BuildQuest2()
    {
        var q = Make("q02_hala_duman", "Hâlâ Duman", "Üç evin boşaldığını Maren anlattı.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.StillSmokeDone });

        q.objectives = new[]
        {
            Collect("q02_obj1", "Demircinin evini ara (Yırtık Sözleşme)", assets != null ? assets.tornContract : null, 1),
            Collect("q02_obj2", "Fırıncının evini ara (Yırtık Sözleşme)", assets != null ? assets.tornContract : null, 1),
            Collect("q02_obj3", "Şifacının evini ara (Yırtık Sözleşme)", assets != null ? assets.tornContract : null, 1),
            Kill("q02_obj4", "Köy dışındaki 2 Gölge Lurker'ı öldür", assets != null ? assets.shadowLurker : null, 2),
            Talk("q02_obj5", "İhtiyar Corvin ile konuş")
        };

        q.itemRewards = assets != null ? new[] { assets.rustedKey } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 80;
        return q;
    }

    QuestData BuildQuest3()
    {
        var q = Make("q03_kirik_muhur", "Kırık Mühür", "Şifacının evindeki sözleşme eksik.", QuestType.Side, flagsOnComplete: new[] { QuestFlags.MissingContractFound });

        q.objectives = new[]
        {
            Talk("q03_obj1", "Köyde 3 farklı NPC ile konuş"),
            Interact("q03_obj2", "Köy meydanında gizli izi takip et"),
            Interact("q03_obj3", "Ahırın arkasındaki kutuyu bul")
        };

        q.itemRewards = assets != null ? new[] { assets.missingContract } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 60;
        return q;
    }

    QuestData BuildQuest4()
    {
        var q = Make("q04_yasli_adamin_itirafi", "Yaşlı Adam'ın İtirafı", "Corvin, akşam seni bekliyor.", QuestType.Main, requiredFlags: new[] { QuestFlags.StillSmokeDone }, flagsOnComplete: new[] { QuestFlags.ElderTruthKnown, QuestFlags.VossContractPlayerAware });

        q.objectives = new[]
        {
            Talk("q04_obj1", "Corvin'i dinle"),
            Interact("q04_obj2", "Eski meydan çeşmesini bul"),
            Interact("q04_obj3", "Çeşmenin altındaki gizli belgeyi al")
        };

        q.itemRewards = assets != null ? new[] { assets.vossDiary } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 120;
        return q;
    }

    QuestData BuildQuest5()
    {
        var q = Make("q05_acik_el", "Açık El", "Defterdeki ilk isim değirmende. Esiri bul ve kurtar.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown }, flagsOnComplete: new[] { QuestFlags.FirstPrisonerRescued }, hasTimeLimit: true, timeLimitSeconds: TwoDaySeconds(), canFail: true);

        q.objectives = new[]
        {
            Interact("q05_obj1", "Köy değirmenini araştır"),
            Interact("q05_obj2", "Değirmencinin anlattığı mağaraya git"),
            Kill("q05_obj3", "Mağaradaki 3 Gölge Bekçisi'ni öldür", assets != null ? assets.shadowGuard : null, 3),
            Interact("q05_obj4", "Mağaradaki esiri kurtar"),
            Interact("q05_obj5", "Esiri köye geri götür")
        };

        q.experienceReward = 200;
        return q;
    }

    QuestData BuildQuest6()
    {
        var q = Make("q06_tarif_defteri", "Tarif Defteri", "Kurtarılan esir, Maren'e eski bir tarif vermek istiyor.", QuestType.Side, requiredFlags: new[] { QuestFlags.FirstPrisonerRescued }, flagsOnComplete: new[] { QuestFlags.MarenRecipeGiven });

        q.objectives = new[]
        {
            Collect("q06_obj1", "Tarlada Tarif Sayfası'nı bul", assets != null ? assets.recipePage : null, 1),
            Talk("q06_obj2", "Tarifi Maren'e götür")
        };

        q.itemRewards = assets != null ? new[] { assets.applePie } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 80;
        return q;
    }

    QuestData BuildQuest7()
    {
        var q = Make("q07_corvinin_borcu", "Corvin'in Borcu", "Eksik sözleşmeyi bulduktan sonra Corvin seni çağırdı.", QuestType.Side, requiredFlags: new[] { QuestFlags.MissingContractFound }, flagsOnComplete: new[] { QuestFlags.CorvinDebtSettled });

        q.objectives = new[]
        {
            Interact("q07_obj1", "Eski kilise harabesine git"),
            Kill("q07_obj2", "Harabedeki 4 Lanetli Bekçi'yi öldür", assets != null ? assets.cursedGuard : null, 4),
            Interact("q07_obj3", "Kasayı aç (Eskimiş Anahtar gerekli)"),
            Collect("q07_obj4", "Kasadaki belgeyi Corvin'e getir", assets != null ? assets.corvinsDocument : null, 1)
        };

        q.itemRewards = assets != null ? new[] { assets.corvinSeal } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 150;
        return q;
    }

    QuestData BuildQuest8()
    {
        var q = Make("q08_defterdeki_sesler", "Defterdeki Sesler", "Voss'un defterini taşıyan birine akşam yabancı biri yaklaştı.", QuestType.Side, requiredFlags: new[] { QuestFlags.VossContractPlayerAware }, flagsOnComplete: new[] { QuestFlags.VossWeakPointKnown });

        q.objectives = new[]
        {
            Talk("q08_obj1", "Gizemli NPC ile konuş"),
            Interact("q08_obj2", "Anlattığı sembolü köyde bul"),
            Interact("q08_obj3", "Corvin'in Mührü'nü sembolün üzerine bas", optional: true)
        };

        q.experienceReward = 100;
        return q;
    }

    QuestData BuildQuest9()
    {
        var q = Make("q09_bekleyis", "Bekleyiş", "Gece köy girişinde Voss'u bekle.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown }, flagsOnComplete: new[] { QuestFlags.Q09VossWarehouseFound });

        q.objectives = new[]
        {
            Interact("q09_obj1", "Köy girişinde Voss'u bekle"),
            Interact("q09_obj2", "Voss'un tezgahından bir şey satın alma", optional: true),
            Interact("q09_obj3", "Voss'u takip et"),
            Interact("q09_obj4", "Voss'un gizli deposunu bul")
        };

        q.experienceReward = 120;
        return q;
    }

    QuestData BuildQuest10()
    {
        var q = Make("q10_acik_hesap", "Açık Hesap", "Voss'un deposuna girdin. Muhafızları bertaraf et, esirleri kurtar, Voss ile yüzleş.", QuestType.Main, requiredFlags: new[] { QuestFlags.ElderTruthKnown, QuestFlags.Q09VossWarehouseFound });

        q.objectives = new[]
        {
            Kill("q10_obj1", "Depodaki 3 Gölge Muhafızı'nı öldür", assets != null ? assets.shadowGuard : null, 3),
            Interact("q10_obj2", "Kafeslerdeki esirleri serbest bırak"),
            Talk("q10_obj3", "Voss ile yüzleş"),
            Kill("q10_obj4", "Voss'u öldür (boss)", assets != null ? assets.vossBoss : null, 1)
        };

        q.experienceReward = 500;
        return q;
    }

    QuestData BuildQuest11()
    {
        var q = Make("q11_fincan_basinda", "Fincan Başında", "Sabah Maren kapında bekliyor.", QuestType.Side, requiredFlags: new[] { QuestFlags.VossDefeatedClean }, flagsOnComplete: new[] { QuestFlags.ScenarioCompleted });

        q.objectives = new[]
        {
            Talk("q11_obj1", "Maren ile konuş"),
            Interact("q11_obj2", "Beraber bahçeye git"),
            Interact("q11_obj3", "Son bir fincan elma çayı iç")
        };

        q.experienceReward = 0;
        q.currencyRewards = new CurrencyReward[0];
        q.itemRewards = new ItemData[0];
        return q;
    }

    QuestData BuildSinnedGuardianQuest1()
    {
        var q = Make("q_sg01_the_debt_that_breathes", "The Debt That Breathes", "Three families. Three names on a list. Your hand, your choice. Shadow Garden believes they are still reachable — held somewhere east of Dragsimo.", QuestType.Main, requiredFlags: new[] { QuestFlags.SinnedGuardianStart }, flagsOnComplete: new[] { QuestFlags.ThreeFamiliesLocated, QuestFlags.ShadowGardenRank1 });

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

        q.itemRewards = assets?.corvinSeal != null ? new[] { assets.corvinSeal } : new ItemData[0];
        q.itemRewardQuantities = new[] { 1 };
        q.experienceReward = 420;
        return q;
    }

    QuestData BuildBoundArchivistQuest1()
    {
        var q = Make("quest_bound_archivist_01", "Bound Archivist", "Brahma'nın bıraktığı dosya, kayıp bir kaydın izini taşıyor. Maren'den başla — Ashenveil'in geçmişi ona sorulur.", QuestType.Main, requiredFlags: new[] { QuestFlags.BoundArchivistStart }, flagsOnComplete: new[] { QuestFlags.BoundArchivistQuest1Done });

        q.objectives = new[]
        {
            Talk("obj_speak_maren_gate", "Kapıda Maren ile konuş"),
            Talk("obj_learn_maren_knowledge", "Maren'in mutfağında geçmişi öğren"),
            Talk("obj_find_elis", "Elis'i bul ve üç konumu öğren", optional: true),
            Talk("obj_handle_eow_operative", "EoW operatifiyle ilgilen"),
            Talk("obj_speak_voss_day3", "3. gün Voss ile konuş"),
            Interact("obj_cross_reference", "Kaydın yerini çapraz referansla daralt")
        };

        q.experienceReward = 380;
        return q;
    }

    QuestData BuildForeignEchoQuest1()
    {
        var q = Make("quest_foreign_echo_01", "Foreign Echo", "Axios anomalisi seni Ashenveil'e çekti. Maren'in kapısındaki gölgeyi takip et — chamber'ın içinde ne olduğunu kimse bilmiyor.", QuestType.Main, requiredFlags: new[] { QuestFlags.ForeignEchoStart }, flagsOnComplete: new[] { QuestFlags.ForeignEchoQuest1Done, QuestFlags.ChamberInteriorSeen });

        q.objectives = new[]
        {
            Interact("obj_follow_shadow_maren", "Maren'in kapısındaki gölgeyi takip et"),
            Talk("obj_understand_maren_need", "Maren'in misyonunu anla"),
            Talk("obj_speak_mireya", "Mireya ile konuş — devriye verisini al", optional: true),
            Talk("obj_meet_chico", "Chico ile buluş", optional: true),
            Talk("obj_speak_voss_day3", "3. gün Voss ile konuş"),
            Interact("obj_identify_axios_anomaly", "Chamber zeminindeki kayıt kaynağını tespit et")
        };

        q.experienceReward = 360;
        return q;
    }
}
