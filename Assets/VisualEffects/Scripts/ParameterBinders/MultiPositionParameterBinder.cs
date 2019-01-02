using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.VFX;
using UnityEngine.VFX.Utils;

[VFXBinder("Utility/Multi-Position Binder")]
public class MultiPositionParameterBinder : VFXBinderBase
{
    [VFXParameterBinding("UnityEngine.Texture2D")]
    public ExposedParameter PositionMapParameter;

    [VFXParameterBinding("System.UInt32")]
    public ExposedParameter CountParameter;

    public GameObject Root;
    public bool EveryFrame = false;

    private Texture2D positionMap;
    private int count;

    public override bool IsValid(VisualEffect component)
    {
        return Root != null && Root.transform.childCount > 0 && component.HasTexture(PositionMapParameter) && component.HasUInt(CountParameter);
    }

    public override void UpdateBinding(VisualEffect component)
    {
        if (EveryFrame)
            UpdateTextures();

        component.SetTexture(PositionMapParameter, positionMap);
        component.SetUInt(CountParameter, (uint)count);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        UpdateTextures();
    }

    void UpdateTextures()
    {
        var candidates = new List<Vector3>();

        foreach(Transform child in Root.transform)
        {
           candidates.Add(child.position);
        }

        count = candidates.Count;

        if(positionMap == null || positionMap.width != count)
        {
            positionMap = new Texture2D(count, 1, TextureFormat.RGBAFloat, false);
        }

        List<Color> colors = new List<Color>();
        foreach(var pos in candidates)
        {
            colors.Add(new Color(pos.x, pos.y, pos.z));
        }
        positionMap.name = Root.name + "_PositionMap";
        positionMap.filterMode = FilterMode.Point;
        positionMap.wrapMode = TextureWrapMode.Clamp;
        positionMap.SetPixels(colors.ToArray(), 0);
        positionMap.Apply();
    }

}
