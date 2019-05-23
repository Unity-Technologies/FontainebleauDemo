using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Light))]
public class CookieCloudAnimation : MonoBehaviour
{
	private Material m_material;
    private CustomRenderTexture m_customRenderTexture;
    private Light m_Light;

	public int size = 256;
    public Shader shader;
    public Texture2D cloudLayer1;
	public Vector2 speedLayer1;
    public Texture2D cloudLayer2;
    public Vector2 speedLayer2;

	// Start is called before the first frame update
	private void OnEnable()
	{
        size = Mathf.Max(1, size);
        m_customRenderTexture = new CustomRenderTexture(size, size);
		m_customRenderTexture.updateMode = CustomRenderTextureUpdateMode.Realtime;
		m_customRenderTexture.useMipMap = true;

        m_Light = gameObject.GetComponent<Light>();
        SetWrapMode();
        m_Light.cookie = m_customRenderTexture;

        CreateMaterial();

        SetShaderProperties();
    }

    public void CreateMaterial()
    {
        if (m_material != null)
            Destroy(m_material);
        if (shader != null)
        {
            m_material = new Material(shader);
            m_customRenderTexture.material = m_material;
        }
    }

    public void SetShaderProperties()
    {
        if (m_material == null)
            CreateMaterial();
        if (m_material == null)
            return;
        m_material.SetTexture("_Tex",cloudLayer1);
		m_material.SetTexture("_Tex2", cloudLayer2);
		m_material.SetVector("_Speed",new Vector4(speedLayer1.x,speedLayer1.y,speedLayer2.x,speedLayer2.y));      
    }

    public void SetWrapMode()
    {
        var wrap = m_Light.type == LightType.Directional ? true : false;

        if (wrap)
            m_customRenderTexture.wrapMode = TextureWrapMode.Repeat;
        if (!wrap)
            m_customRenderTexture.wrapMode = TextureWrapMode.Clamp;
    }

	private void OnDisable()
	{
        OnDestroy();
	}

	private void OnDestroy()
	{
        m_Light.cookie = null;
        if(m_customRenderTexture != null)
            m_customRenderTexture.Release();
        if(Application.isPlaying)
        {
            if (m_customRenderTexture != null)
                Destroy(m_customRenderTexture);
            if (m_material != null)
                Destroy(m_material);
        }
        else
        {
            if (m_customRenderTexture != null)
                DestroyImmediate(m_customRenderTexture);
            if (m_material != null)
                DestroyImmediate(m_material);
        }
	}
}
