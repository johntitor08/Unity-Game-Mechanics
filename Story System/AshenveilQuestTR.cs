using System.Collections.Generic;

public static class AshenveilQuestTR
{
    public struct Q
    {
        public string name;
        public string desc;

        public Q(string n, string d)
        {
            name = n;
            desc = d;
        }
    }

    static readonly Dictionary<string, Q> Quests = new()
    {
        { "q01_bir_fincan_huzur", new Q("Bir Fincan Huzur", "Maren seni sabah erkenden çağırdı.") },
        { "q02_hala_duman", new Q("Hâlâ Duman", "Maren sana üç evin boşaltıldığını söyledi.") },
        { "q03_kirik_muhur", new Q("Kırık Mühür", "Şifacının evindeki sözleşmenin bir parçası eksik.") },
        { "q04_yasli_adamin_itirafi", new Q("Yaşlı Adamın İtirafı", "Corvin akşam seni bekliyor.") },
        { "q05_acik_el", new Q("Açık El", "Defterdeki ilk isim değirmende. Tutsağı bul ve kurtar.") },
        { "q06_tarif_defteri", new Q("Tarif Defteri", "Kurtarılan tutsak Maren'e eski bir tarif vermek istiyor.") },
        { "q07_corvinin_borcu", new Q("Corvin'in Borcu", "Kayıp sözleşmeyi bulduktan sonra Corvin seni çağırdı.") },
        { "q08_defterdeki_sesler", new Q("Defterdeki Sesler", "Akşam, Voss'un defterini taşıyana bir yabancı yaklaştı.") },
        { "q09_bekleyis", new Q("Bekleyiş", "Gece köy girişinde Voss'u bekle.") },
        { "q10_acik_hesap", new Q("Açık Hesap", "Voss'un deposuna girdin. Muhafızları etkisiz hâle getir, tutsakları serbest bırak ve Voss'la yüzleş.") },
        { "q11_fincan_basinda", new Q("Fincan Başında", "Sabah, Maren kapında bekliyor.") },
        { "q_sg01_the_debt_that_breathes", new Q("Nefes Alan Borç", "Üç aile. Bir listede üç isim. Senin elin, senin seçimin. Shadow Garden hâlâ ulaşılabilir olduklarına inanıyor — Dragsimo'nun doğusunda bir yerde tutuluyorlar.") },
        { "q_ba01_the_reversal_clause", new Q("Geri Alma Maddesi", "Brahma'nın ardında bıraktığı dosya, kayıp bir kaydın izini taşıyor. Maren'le başla — Ashenveil'in geçmişi ona sorulur.") },
        { "q_fe01_the_originating_record", new Q("Köken Kaydı", "Axios anomalisi seni Ashenveil'e çekti. Maren'in kapısındaki gölgeyi takip et — odanın içinde ne olduğunu kimse bilmiyor.") },
    };

    static readonly Dictionary<string, string> Objectives = new()
    {
        { "q01_obj1", "Bahçeden 1 Taze Elma topla" }, { "q01_obj2", "Mutfak rafından 1 Tarçın al" },
        { "q01_obj3", "Ocakta çayı demle" }, { "q01_obj4", "Fincanı Maren'e götür" },
        { "q02_obj1", "Demircinin evini ara (Yırtık Sözleşme)" }, { "q02_obj2", "Fırıncının evini ara (Yırtık Sözleşme)" },
        { "q02_obj3", "Şifacının evini ara (Yırtık Sözleşme)" }, { "q02_obj4", "Köy dışında 2 Gölge Sinsi öldür" },
        { "q02_obj5", "Yaşlı Corvin ile konuş" },
        { "q03_obj1", "Köyde 3 farklı NPC ile konuş" }, { "q03_obj2", "Köy meydanındaki gizli izi takip et" },
        { "q03_obj3", "Ahırın arkasındaki kutuyu bul" },
        { "q04_obj1", "Corvin'i dinle" }, { "q04_obj2", "Eski meydan çeşmesini bul" },
        { "q04_obj3", "Çeşmenin altındaki gizli belgeyi al" },
        { "q05_obj1", "Köy değirmenini araştır" }, { "q05_obj2", "Değirmencinin bahsettiği mağaraya git" },
        { "q05_obj3", "Mağaradaki 3 Gölge Muhafızı öldür" }, { "q05_obj4", "Mağaradaki tutsağı kurtar" },
        { "q05_obj5", "Tutsağı köye geri götür" },
        { "q06_obj1", "Tarladaki Tarif Sayfasını bul" }, { "q06_obj2", "Tarifi Maren'e götür" },
        { "q07_obj1", "Eski kilise harabelerine git" }, { "q07_obj2", "Harabelerdeki 3 Lanetli Muhafızı öldür" },
        { "q07_obj3", "Sandığı aç (Paslı Anahtar gerekli)" }, { "q07_obj4", "Sandıktaki belgeyi Corvin'e getir" },
        { "q08_obj1", "Gizemli NPC ile konuş" }, { "q08_obj2", "Köyde tarif ettikleri sembolü bul" },
        { "q08_obj3", "Corvin'in Mührünü sembole bastır" },
        { "q09_obj1", "Köy girişinde Voss'u bekle" }, { "q09_obj2", "Voss'un tezgâhından hiçbir şey alma" },
        { "q09_obj3", "Voss'u takip et" }, { "q09_obj4", "Voss'un gizli deposunu bul" },
        { "q10_obj1", "Depodaki 3 Gölge Muhafızı öldür" }, { "q10_obj2", "Kafeslerdeki tutsakları serbest bırak" },
        { "q10_obj3", "Voss'la yüzleş" }, { "q10_obj4", "Voss'u öldür (boss)" },
        { "q11_obj1", "Maren ile konuş" }, { "q11_obj2", "Birlikte bahçeye git" },
        { "q11_obj3", "Son bir fincan elma çayı iç" },
        { "q_sg01_obj1", "Gece Maren ile konuş" }, { "q_sg01_obj2", "Kuyuda Aslude ile buluş" },
        { "q_sg01_obj3", "Corvin ile konuş (isteğe bağlı)" }, { "q_sg01_obj4", "Köyün batı ucunu öğren (3 konumdan 2'si)" },
        { "q_sg01_obj5", "Voss'u batı yolundan takip et" }, { "q_sg01_obj6", "Voss ile konuş" },
        { "q_sg01_obj7", "Dragsimo'nun doğusundaki ikinci duvarı bul" }, { "q_sg01_obj8", "İkinci duvarın ardındakini incele" },
        { "q_sg01_obj9", "Keşifle Aslude'a dön" },
        { "obj_speak_maren_gate", "Kapıda Maren ile konuş" }, { "obj_learn_maren_knowledge", "Maren'in mutfağında geçmişi öğren" },
        { "obj_find_elis", "Elis'i bul ve üç konumu öğren" }, { "obj_handle_eow_operative", "EoW ajanıyla ilgilen" },
        { "obj_speak_voss_day3", "3. gün Voss ile konuş" }, { "obj_cross_reference", "Çapraz referansla kaydın konumunu daralt" },
        { "obj_follow_shadow_maren", "Maren'in kapısındaki gölgeyi takip et" }, { "obj_understand_maren_need", "Maren'in görevini anla" },
        { "obj_speak_mireya", "Mireya ile konuş — devriye verisini al" }, { "obj_meet_chico", "Chico ile buluş" },
        { "obj_identify_axios_anomaly", "Oda zemininde kayıt kaynağını belirle" },
    };

    public static string NameTR(string questID) => Quests.TryGetValue(questID, out var q) ? q.name : "";

    public static string DescTR(string questID) => Quests.TryGetValue(questID, out var q) ? q.desc : "";

    public static string ObjTR(string objectiveID) => Objectives.TryGetValue(objectiveID, out var d) ? d : "";
}
