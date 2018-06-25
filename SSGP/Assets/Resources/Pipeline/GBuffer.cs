using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GBuffer
{
    public RenderTargetIdentifier[] layerIdentifiers;
    public RenderTargetIdentifier depthIdentifier;

    public RenderTexture[] colorAttachments;
    public RenderTexture depthAttachment;

    public int w, h;
    
    public GBuffer(int width, int height, int depthBits, RenderTextureFormat[] layerFormats)
    {
        w = width;
        h = height;

        colorAttachments = new RenderTexture[layerFormats.Length];
        layerIdentifiers = new RenderTargetIdentifier[layerFormats.Length];

        for (int i = 0; i < layerFormats.Length; i++)
        {
            colorAttachments[i] = new RenderTexture(width, height, 0, layerFormats[i]);
            colorAttachments[i].Create();
            layerIdentifiers[i] = colorAttachments[i];
        }

        depthAttachment = new RenderTexture(width, height, depthBits, RenderTextureFormat.Depth);
         
        depthIdentifier = depthAttachment;
    }

    public void Release()
    {
        foreach (RenderTexture t in colorAttachments)
        {
            if (t != null)
                t.Release();
        }

        if (depthAttachment != null)
            depthAttachment.Release();
    }
}
