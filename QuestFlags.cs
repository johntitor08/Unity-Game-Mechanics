public static class QuestFlags
{
    public const string OriginPrefix = "origin:";
    public const string LegacyBoundArchivistStart = "archivist_start";
    public const string LegacyForeignEchoStart = "echo_start";
    public const string AshenveilEntered = "ashenveil_entered";
    public const string BoundArchivistStart = "bound_archivist_start";
    public const string BoundArchivistOpeningComplete = "bound_archivist_opening_complete";
    public const string BrahmaLeft = "brahma_left";
    public const string SealedFileTaken = "sealed_file_taken";
    public const string MarenMetBoundArchivist = "maren_met_bound_archivist";
    public const string MarenDialogueDone = "maren_dialogue_done";
    public const string ReversalClauseKnown = "reversal_clause_known";
    public const string ChurchRuinsClue = "church_ruins_clue";
    public const string ElisMet = "elis_met";
    public const string ThreeLocationsKnown = "three_locations_known";
    public const string EowOperativeMet = "eow_operative_met";
    public const string EowWatching = "eow_watching";
    public const string BoundArchivistEowToken = "bound_archivist_eow_token";
    public const string VossDay3BoundArchivist = "voss_day3_bound_archivist";
    public const string RecordNotChurchConfirmed = "record_not_church_confirmed";
    public const string VossMovedRecord = "voss_moved_record";
    public const string RecordLocationNarrowed = "record_location_narrowed";
    public const string WitnessRequiredKnown = "witness_required_known";
    public const string BoundArchivistQuest1Done = "bound_archivist_quest1_done";
    public const string BoundArchivistQ1EowInvited = "bound_archivist_q1_eow_invited";
    public const string ForeignEchoStart = "foreign_echo_start";
    public const string ForeignEchoOpeningComplete = "foreign_echo_opening_complete";
    public const string ShadowAnomalySeen = "shadow_anomaly_seen";
    public const string MarenMetForeignEcho = "maren_met_foreign_echo";
    public const string VossCantIdentifyForeignEcho = "voss_cant_identify_foreign_echo";
    public const string AxiosResonanceExplained = "axios_resonance_explained";
    public const string MarenMissionGiven = "maren_mission_given";
    public const string ForeignEchoInvisibleToTracking = "foreign_echo_invisible_to_tracking";
    public const string ChamberTargetKnown = "chamber_target_known";
    public const string MireyaMetForeignEcho = "mireya_met_foreign_echo";
    public const string LurkerPatrolData = "lurker_patrol_data";
    public const string ChicoMet = "chico_met";
    public const string AwamoriIncomingKnown = "awamori_incoming_known";
    public const string AxiosFrequencyShared = "axios_frequency_shared";
    public const string VossDay3ForeignEcho = "voss_day3_foreign_echo";
    public const string ChamberFrequencyWarning = "chamber_frequency_warning";
    public const string VossCannotTrackForeignEcho = "voss_cannot_track_foreign_echo";
    public const string ThreeCrystalsSeen = "three_crystals_seen";
    public const string FourthSlotResonatesForeignEcho = "fourth_slot_resonates_foreign_echo";
    public const string OriginatingRecordBelow = "originating_record_below";
    public const string ForeignEchoQuest1Done = "foreign_echo_quest1_done";
    public const string ChamberInteriorSeen = "chamber_interior_seen";
    public const string AxiosAnomalyIdentified = "axios_anomaly_identified";
    public const string SinnedGuardianStart = "sinned_guardian_start";
    public const string SinnedGuardianOpeningComplete = "sinned_guardian_opening_complete";
    public const string MarenFirstMeeting = "maren_first_meeting";
    public const string MarenToldGuardianMission = "maren_told_guardian_mission";
    public const string VossCollectsNotDestroysKnown = "voss_collects_not_destroys_known";
    public const string ShadowGardenQuestAssigned = "shadow_garden_quest_assigned";
    public const string AsludeMet = "aslude_met";
    public const string VossReturnsDay3Known = "voss_returns_day3_known";
    public const string CorvinOptionalSpoken = "corvin_optional_spoken";
    public const string DragsimEastClue = "dragsimo_east_clue";
    public const string VossDay3Guardian = "voss_day3_guardian";
    public const string VossTrackedGuardian = "voss_tracked_guardian";
    public const string DragsimSecondWallKnown = "dragsimo_second_wall_known";
    public const string VossReversalClauseHinted = "voss_reversal_clause_hinted";
    public const string ThreeFamiliesLocated = "three_families_located";
    public const string FourthCrystalSlotSeen = "fourth_crystal_slot_seen";
    public const string VossPreparedGuardianContract = "voss_prepared_guardian_contract";
    public const string SinnedGuardianQuest1Done = "sinned_guardian_quest1_done";
    public const string ShadowGardenRank1 = "shadow_garden_rank1";
    public const string VossWeakPointKnown = "voss_weak_point_known";
    public const string VossWeakPointApplied = "voss_weak_point_applied";
    public const string MarenTeaServed = "maren_tea_served";
    public const string StillSmokeDone = "still_smoke_done";
    public const string MissingContractFound = "missing_contract_found";
    public const string ElderTruthKnown = "elder_truth_known";
    public const string VossContractPlayerAware = "voss_contract_player_aware";
    public const string FirstPrisonerRescued = "first_prisoner_rescued";
    public const string MarenRecipeGiven = "maren_recipe_given";
    public const string CorvinDebtSettled = "corvin_debt_settled";
    public const string Q09VossWarehouseFound = "q09_voss_warehouse_found";
    public const string VossDefeatedClean = "voss_defeated_clean";
    public const string ScenarioCompleted = "scenario_completed";

    public static void MigrateLegacyOriginStartFlags()
    {
        if (StoryFlags.Has(LegacyBoundArchivistStart) && !StoryFlags.Has(BoundArchivistStart))
            StoryFlags.Add(BoundArchivistStart);

        if (StoryFlags.Has(LegacyForeignEchoStart) && !StoryFlags.Has(ForeignEchoStart))
            StoryFlags.Add(ForeignEchoStart);
    }
}
