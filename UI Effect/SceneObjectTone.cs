using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class SceneObjectTone : MonoBehaviour
{
    static readonly Dictionary<string, Color> Tints = new()
    {
        { "bg00_evening", new Color(0.930f, 0.667f, 0.530f) },
        { "bg00_morning", new Color(1.076f, 0.887f, 0.770f) },
        { "bg00_night", new Color(0.684f, 0.483f, 0.403f) },
        { "bg02_evening", new Color(0.744f, 0.498f, 0.350f) },
        { "bg02_morning", new Color(1.086f, 1.084f, 0.980f) },
        { "bg02_night", new Color(0.481f, 0.448f, 0.421f) },
        { "bg03_evening", new Color(0.700f, 0.526f, 0.435f) },
        { "bg03_morning", new Color(1.085f, 1.086f, 0.979f) },
        { "bg03_night", new Color(0.500f, 0.532f, 0.568f) },
        { "bg04_morning", new Color(1.054f, 0.942f, 0.880f) },
        { "bg05_morning", new Color(0.845f, 0.688f, 0.553f) },
        { "bg06_morning", new Color(1.100f, 0.954f, 0.907f) },
        { "bg07_morning", new Color(1.100f, 1.028f, 0.962f) },
        { "bg08_evening", new Color(1.100f, 1.017f, 0.889f) },
        { "bg08_morning", new Color(1.097f, 1.048f, 1.004f) },
        { "bg08_night", new Color(1.079f, 1.014f, 0.979f) },
        { "bg09_evening", new Color(1.097f, 0.970f, 0.852f) },
        { "bg09_morning", new Color(1.059f, 1.049f, 1.042f) },
        { "bg09_night", new Color(0.561f, 0.626f, 0.754f) },
        { "bg10_evening", new Color(1.053f, 0.855f, 0.725f) },
        { "bg10_morning", new Color(1.100f, 0.988f, 0.908f) },
        { "bg10_night", new Color(0.849f, 0.782f, 0.743f) },
        { "bg11_evening", new Color(1.100f, 1.020f, 0.890f) },
        { "bg11_morning", new Color(1.100f, 1.034f, 0.953f) },
        { "bg11_night", new Color(0.682f, 0.570f, 0.535f) },
        { "bg12_evening", new Color(1.100f, 1.044f, 0.938f) },
        { "bg12_morning", new Color(1.100f, 1.048f, 0.997f) },
        { "bg12_night", new Color(0.997f, 0.908f, 0.851f) },
        { "bg13_evening", new Color(1.059f, 0.870f, 0.752f) },
        { "bg13_morning", new Color(1.100f, 1.010f, 0.952f) },
        { "bg13_night", new Color(0.674f, 0.735f, 0.868f) },
        { "bg14_evening", new Color(1.091f, 0.930f, 0.806f) },
        { "bg14_morning", new Color(1.100f, 1.045f, 0.985f) },
        { "bg14_night", new Color(0.488f, 0.488f, 0.574f) },
        { "bg15_evening", new Color(0.918f, 0.816f, 0.659f) },
        { "bg15_morning", new Color(0.952f, 0.925f, 0.806f) },
        { "bg15_night", new Color(0.523f, 0.495f, 0.484f) },
        { "bg16_evening", new Color(1.100f, 1.023f, 0.906f) },
        { "bg16_morning", new Color(1.100f, 1.046f, 0.989f) },
        { "bg16_night", new Color(0.957f, 0.887f, 0.850f) },
        { "bg17_evening", new Color(1.047f, 0.909f, 0.785f) },
        { "bg17_morning", new Color(1.096f, 1.031f, 0.958f) },
        { "bg17_night", new Color(0.495f, 0.478f, 0.546f) },
        { "bg18_morning", new Color(1.100f, 1.047f, 0.994f) },
        { "bg19_morning", new Color(1.018f, 0.893f, 0.831f) },
        { "bg20_evening", new Color(1.031f, 0.864f, 0.748f) },
        { "bg20_morning", new Color(1.071f, 0.966f, 0.894f) },
        { "bg20_night", new Color(0.516f, 0.528f, 0.620f) },
        { "bg21_morning", new Color(0.998f, 0.901f, 0.829f) },
        { "bg22_morning", new Color(1.100f, 1.043f, 0.973f) },
        { "bg23_morning", new Color(1.100f, 0.998f, 0.903f) },
        { "bg24_morning", new Color(1.069f, 0.967f, 0.903f) },
        { "bg25_morning", new Color(1.067f, 0.985f, 0.923f) },
        { "bg26_morning", new Color(1.076f, 0.856f, 0.735f) },
        { "bg27_morning", new Color(0.813f, 0.645f, 0.502f) },
        { "bg27_night", new Color(0.459f, 0.447f, 0.444f) },
        { "bg28_morning", new Color(1.100f, 1.052f, 0.982f) },
        { "bg29_morning", new Color(1.100f, 1.050f, 0.984f) },
        { "bg30_morning", new Color(0.956f, 0.903f, 0.839f) },
        { "bg31_morning", new Color(1.049f, 0.963f, 0.878f) },
        { "bg32_morning", new Color(0.913f, 1.079f, 1.100f) },
        { "bg33_morning", new Color(1.023f, 0.709f, 0.678f) },
        { "bg34_morning", new Color(1.100f, 1.008f, 0.844f) },
        { "bg35_morning", new Color(0.699f, 0.998f, 0.967f) },
        { "bg36_morning", new Color(0.679f, 0.920f, 1.049f) },
        { "bg37_morning", new Color(0.832f, 0.663f, 0.630f) },
        { "bg38_evening", new Color(1.051f, 0.999f, 0.720f) },
        { "bg38_morning", new Color(1.086f, 1.100f, 0.837f) },
        { "bg38_night", new Color(0.964f, 0.916f, 0.779f) },
        { "bg39_evening", new Color(1.013f, 0.964f, 0.852f) },
        { "bg39_morning", new Color(1.040f, 1.073f, 1.037f) },
        { "bg39_night", new Color(0.804f, 0.802f, 0.829f) },
        { "bg40_evening", new Color(0.990f, 0.947f, 0.819f) },
        { "bg40_morning", new Color(1.034f, 1.070f, 1.012f) },
        { "bg40_night", new Color(0.786f, 0.808f, 0.833f) },
        { "bg41_morning", new Color(0.862f, 0.751f, 0.870f) },
        { "bg42_morning", new Color(1.007f, 0.862f, 0.883f) },
        { "bg42_night", new Color(0.899f, 0.798f, 0.772f) },
        { "bg43_morning", new Color(1.082f, 1.021f, 0.976f) },
        { "bg44_morning", new Color(1.092f, 1.046f, 1.013f) },
        { "bg_apple_garden_0", new Color(0.984f, 0.986f, 0.882f) },
        { "bg_apple_garden_evening_00001_", new Color(0.947f, 0.879f, 0.723f) },
        { "bg_apple_garden_night", new Color(0.513f, 0.543f, 0.610f) },
        { "bg_baker_home_0", new Color(1.031f, 0.910f, 0.843f) },
        { "bg_baker_home_evening_00001_", new Color(0.981f, 0.779f, 0.656f) },
        { "bg_baker_home_night", new Color(0.508f, 0.509f, 0.590f) },
        { "bg_barn_0", new Color(1.086f, 0.922f, 0.800f) },
        { "bg_barn_evening_00001_", new Color(1.053f, 0.846f, 0.697f) },
        { "bg_barn_night", new Color(0.494f, 0.499f, 0.581f) },
        { "bg_blacksmith_home_0", new Color(0.849f, 0.768f, 0.738f) },
        { "bg_blacksmith_home_evening_00001_", new Color(0.811f, 0.650f, 0.557f) },
        { "bg_blacksmith_home_night", new Color(0.413f, 0.426f, 0.517f) },
        { "bg_cave_0", new Color(0.661f, 0.727f, 0.834f) },
        { "bg_cave_evening_00001_", new Color(0.625f, 0.611f, 0.603f) },
        { "bg_cave_night", new Color(0.370f, 0.416f, 0.564f) },
        { "bg_church_ruins_0", new Color(0.741f, 0.838f, 0.872f) },
        { "bg_church_ruins_evening_00001_", new Color(0.692f, 0.700f, 0.632f) },
        { "bg_church_ruins_night", new Color(0.387f, 0.439f, 0.562f) },
        { "bg_forest_0", new Color(0.872f, 0.848f, 0.719f) },
        { "bg_forest_evening_00001_", new Color(0.877f, 0.778f, 0.680f) },
        { "bg_forest_night", new Color(0.548f, 0.600f, 0.740f) },
        { "bg_forest_noon", new Color(0.882f, 0.858f, 0.720f) },
        { "bg_healer_home_0", new Color(0.841f, 0.706f, 0.622f) },
        { "bg_healer_home_evening_00001_", new Color(0.811f, 0.602f, 0.480f) },
        { "bg_healer_home_night", new Color(0.428f, 0.425f, 0.497f) },
        { "bg_maren_kitchen_0", new Color(0.974f, 0.826f, 0.739f) },
        { "bg_maren_kitchen_evening_00001_", new Color(0.937f, 0.717f, 0.587f) },
        { "bg_maren_kitchen_night", new Color(0.471f, 0.466f, 0.536f) },
        { "bg_stove_00001_", new Color(0.980f, 0.805f, 0.703f) },
        { "bg_stove_evening", new Color(0.943f, 0.703f, 0.570f) },
        { "bg_stove_night", new Color(0.478f, 0.463f, 0.522f) },
        { "bg_templeborn_archives_00001_", new Color(0.976f, 0.862f, 0.796f) },
        { "bg_templeborn_archives_evening_00001_", new Color(0.943f, 0.766f, 0.654f) },
        { "bg_templeborn_archives_night", new Color(0.554f, 0.532f, 0.568f) },
        { "bg_village_fields_0", new Color(0.931f, 0.866f, 0.760f) },
        { "bg_village_fields_evening_00001_", new Color(0.901f, 0.776f, 0.635f) },
        { "bg_village_fields_night", new Color(0.438f, 0.461f, 0.546f) },
        { "bg_village_gate_0", new Color(0.785f, 0.734f, 0.702f) },
        { "bg_village_gate_evening_00001_", new Color(0.766f, 0.649f, 0.562f) },
        { "bg_village_gate_night", new Color(0.418f, 0.426f, 0.506f) },
        { "bg_village_mill_0", new Color(0.895f, 0.877f, 0.820f) },
        { "bg_village_mill_evening_00001_", new Color(0.862f, 0.773f, 0.660f) },
        { "bg_village_mill_night", new Color(0.444f, 0.473f, 0.565f) },
        { "bg_village_square_0", new Color(0.924f, 0.865f, 0.892f) },
        { "bg_village_square_evening_00001_", new Color(0.894f, 0.769f, 0.719f) },
        { "bg_village_square_night", new Color(0.456f, 0.473f, 0.589f) },
        { "bg_voss_stall_00001_", new Color(0.854f, 0.770f, 0.738f) },
        { "bg_voss_stall_evening_00001_", new Color(0.825f, 0.674f, 0.587f) },
        { "bg_voss_stall_night", new Color(0.422f, 0.432f, 0.518f) },
        { "bg_voss_warehouse_00001_", new Color(0.743f, 0.662f, 0.645f) },
        { "bg_voss_warehouse_evening_00001_", new Color(0.722f, 0.570f, 0.495f) },
        { "bg_voss_warehouse_night", new Color(0.401f, 0.419f, 0.530f) },
        { "scene_2_bg_00002_", new Color(1.026f, 0.907f, 0.837f) },
    };

    Image image;
    Color baseColor;
    Sprite lastBackground;

    void Awake()
    {
        image = GetComponent<Image>();
        baseColor = image.color;
    }

    void OnEnable() => Apply(true);

    void LateUpdate() => Apply(false);

    void Apply(bool force)
    {
        if (image == null || SceneEvent.Instance == null || SceneEvent.Instance.backgroundImage == null)
            return;

        Sprite bg = SceneEvent.Instance.backgroundImage.sprite;

        if (!force && bg == lastBackground)
            return;

        lastBackground = bg;
        Color tint = bg != null && Tints.TryGetValue(bg.name, out var t) ? t : Color.white;
        image.color = new Color(baseColor.r * tint.r, baseColor.g * tint.g, baseColor.b * tint.b, baseColor.a);
    }
}
