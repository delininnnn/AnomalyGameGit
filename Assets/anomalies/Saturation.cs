using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Saturanion : MonoBehaviour
{
    private ColorAdjustments colorAdjustments;

    void Start()
    {
        // Íàõîäèì Volume è ïîëó÷àåì ColorAdjustments
        Volume volume = FindObjectOfType<Volume>();
        if (volume != null)
            volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
    }

    // İÒÎÒ ÌÅÒÎÄ ÌÛ ÁÓÄÅÌ ÏĞÈÂßÇÛÂÀÒÜ Ê ÑËÀÉÄÅĞÓ
    public void SetSaturation(float value)
    {
        if (colorAdjustments != null)
            colorAdjustments.saturation.value = value;
    }
}