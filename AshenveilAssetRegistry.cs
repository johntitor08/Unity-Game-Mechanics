using UnityEngine;

[CreateAssetMenu(fileName = "AshenveilAssetRegistry", menuName = "Story/Ashenveil")]
public class AshenveilAssetRegistry : ScriptableObject
{
    [Header("Items — Ashenveil Core")]
    public ItemData freshApple;
    public ItemData cinnamon;
    public ItemData appleTeaSeed;
    public ItemData appleTea;
    public ItemData tornContract;
    public ItemData rustedKey;
    public ItemData missingContract;
    public ItemData vossDiary;
    public ItemData recipePage;
    public ItemData applePie;
    public ItemData corvinsDocument;
    public ItemData corvinSeal;

    [Header("Items — Sinned Guardian Origin")]
    public ItemData corvinsTestimony;
    public ItemData axiosCrystal;

    [Header("Enemies")]
    public EnemyData shadowLurker;
    public EnemyData shadowGuard;
    public EnemyData cursedGuard;
    public EnemyData vossBoss;
}
