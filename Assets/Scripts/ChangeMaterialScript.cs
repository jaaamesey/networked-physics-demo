using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMaterialScript : MonoBehaviour
{
    //Vito Colavito 12022036
    MeshRenderer meshRenderer;

    public Texture albedoTexture;
    public Texture normalTexture;

    float rColorValue = 1f;
    float gColorValue = 1f;
    float bColorValue = 1f;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void Update()
    {
        /*if (meshRenderer == null && GameManager.GetCurrent().CurrentPlayer != null)
            meshRenderer = GameManager.GetCurrent().CurrentPlayer.transform.Find("CharacterMesh")
                .GetComponent<MeshRenderer>();
            print(meshRenderer.material);*/
    }

    public void RColorValue(float rColor)
    {
        rColorValue = rColor;
        meshRenderer.material.SetColor("_Color", new Vector4(rColorValue, gColorValue, bColorValue, 1f));
    }

    public void GColorValue(float gColor)
    {
        gColorValue = gColor;
        meshRenderer.material.SetColor("_Color", new Vector4(rColorValue, gColorValue, bColorValue, 1f));
    }

    public void BColorValue(float bColor)
    {
        bColorValue = bColor;
        meshRenderer.material.SetColor("_Color", new Vector4(rColorValue, gColorValue, bColorValue, 1f));
    }

    public void ChangeAlbedoTexture()
    {
        meshRenderer.material.SetTexture("_MainTex", albedoTexture);
    }

    public void ChangeNormalTexture()
    {
        meshRenderer.material.SetTexture("_BumpMap", normalTexture);
    }

    public void MetallicSlider(float metallic)
    {
        meshRenderer.material.SetFloat("_Metallic", metallic);
    }

    public void SmoothnessSlider(float smoothness)
    {
        meshRenderer.material.SetFloat("_Glossiness", smoothness);
    }
}