using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEditor;
using UnityEditor.AnimatedValues;

[ExecuteInEditMode]
public class LightingPipelineAsset : RenderPipelineAsset
{
#if UNITY_EDITOR

    public int ShadowmapResolution = 2048;
    public float ShadowBleedDistance = 0.5f;
    public Shader pointLightShader;
    public Shader pointOccluderShader;
    public Shader pointMeshOccluderShader;

    [UnityEditor.MenuItem("CONTEXT/GraphicsSettings/wew")]
    static void TestMenuPlease()
    {
        MonoBehaviour.print("ayyyyyyyyy"); 
    }
     
    [UnityEditor.MenuItem("Custom Pipeline/Create Lighting Asset")]
    static void CreateLightingPipeline()
    {
        var instance = ScriptableObject.CreateInstance<LightingPipelineAsset>();
        UnityEditor.AssetDatabase.CreateAsset(instance, "Assets/Resources/Pipeline/LightingPipelineAsset.asset");
    }

    [UnityEditor.MenuItem("Debug View/None")]
    static void GDebug_None()
    {
        LightingPipeline.graphicsDebugMode = 0;
    }

    [UnityEditor.MenuItem("Debug View/GBuffer 0 (Color)")]
    static void GDebug_GB0()
    {
        LightingPipeline.graphicsDebugMode = 1;
    }

    [UnityEditor.MenuItem("Debug View/GBuffer 1 (World Normal)")]
    static void GDebug_GB1()
    {
        LightingPipeline.graphicsDebugMode = 2;
    }

    [UnityEditor.MenuItem("Debug View/GBuffer 2 (Metalness, Roughness)")]
    static void GDebug_GB2()
    {
        LightingPipeline.graphicsDebugMode = 3;
    }
#endif

    void CreateLightMaterials()
    {
        MonoBehaviour.print("Created light materials");
        if (pointLightShader != null)
        {
            LightingPipeline.lightMaterials[0] = new Material(pointLightShader);
            LightingPipeline.lightMaterials[0].enableInstancing = true;

            //This is necessary because Unity will cap the size of the array to the first value you give it
            //Only have 5 lights visible? Wanna add a 6th one?
            //Too bad! The first array you gave it was of length 5, so you can now only give it 5 lights permanently.
            Vector4[] lightColors = new Vector4[100];
            for (int i = 0; i < 100; i++)
            {
                lightColors[i] = new Vector4();
            }
            LightingPipeline.lightMaterials[0].SetVectorArray("_LightColor", lightColors);
        }
        if (pointOccluderShader != null)
        {
            LightingPipeline.occluderMaterials[0] = new Material(pointOccluderShader);
            LightingPipeline.occluderMaterials[0].enableInstancing = true;
            LightingPipeline.shmapResolution = ShadowmapResolution;
            LightingPipeline.shadowBleedDistance = ShadowBleedDistance;
            LightingPipeline.occluderMaterials[0].SetVector("_TexelSize", new Vector4(1.0f / ShadowmapResolution, 1.0f / ShadowmapResolution, ShadowmapResolution, ShadowmapResolution));
        }

        if (pointMeshOccluderShader != null)
        {
            LightingPipeline.occluderMaterials[1] = new Material(pointMeshOccluderShader);
            LightingPipeline.occluderMaterials[1].enableInstancing = true;
            LightingPipeline.shmapResolution = ShadowmapResolution;
            LightingPipeline.shadowBleedDistance = ShadowBleedDistance;
            LightingPipeline.occluderMaterials[1].SetVector("_TexelSize", new Vector4(1.0f / ShadowmapResolution, 1.0f / ShadowmapResolution, ShadowmapResolution, ShadowmapResolution));
        }
    }

    void CreateShadowmap()
    {
        if (LightingPipeline.shadowMap != null)
            LightingPipeline.shadowMap.Release();

        LightingPipeline.shadowMap = new RenderTexture(ShadowmapResolution, ShadowmapResolution, 16, RenderTextureFormat.ARGBFloat);
        LightingPipeline.shadowMap.filterMode = FilterMode.Bilinear;
        LightingPipeline.shadowMap.wrapModeV = TextureWrapMode.Repeat;
        LightingPipeline.shadowMap.wrapModeU = TextureWrapMode.Clamp;
        LightingPipeline.shadowMap.Create();
    }

    void CreateQuad()
    {
        LightingPipeline.quad = new Mesh();

        List<Vector3> quadVerts = new List<Vector3>();
        quadVerts.Add(new Vector3(-1.0f, -1.0f, 0.0f));
        quadVerts.Add(new Vector3(1.0f, -1.0f, 0.0f));
        quadVerts.Add(new Vector3(1.0f, 1.0f, 0.0f));
        quadVerts.Add(new Vector3(-1.0f, 1.0f, 0.0f));

        LightingPipeline.quad.SetVertices(quadVerts);

        int[] indices = new int[6];

        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;

        List<Vector2> quadUVs = new List<Vector2>();

        quadUVs.Add(new Vector2(0.0f, 0.0f));
        quadUVs.Add(new Vector2(1.0f, 0.0f));
        quadUVs.Add(new Vector2(1.0f, 1.0f));
        quadUVs.Add(new Vector2(0.0f, 1.0f));

        LightingPipeline.quad.SetIndices(indices, MeshTopology.Triangles, 0);

        LightingPipeline.quad.SetUVs(0, quadUVs);

        LightingPipeline.quad.UploadMeshData(false);
    }

    void CreateBoxSides()
    {
        LightingPipeline.boxSides = new Mesh();

        List<Vector3> normals = new List<Vector3>();

        //Each face has its own normal vector, so the verts can't be shared
        //Basically, that just means each vertex has to exist twice in the array

        List<Vector3> verts = new List<Vector3>();
        verts.Add(new Vector3(-1.0f, -1.0f, 1.0f));
        verts.Add(new Vector3(1.0f, -1.0f, 1.0f));
        verts.Add(new Vector3(1.0f, -1.0f, -1.0f));
        verts.Add(new Vector3(-1.0f, -1.0f, -1.0f));
        normals.Add(new Vector3(0.0f, -1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, -1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, -1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, -1.0f, 0.0f));

        verts.Add(new Vector3(1.0f, -1.0f, 1.0f));
        verts.Add(new Vector3(1.0f, 1.0f, 1.0f));
        verts.Add(new Vector3(1.0f, 1.0f, -1.0f));
        verts.Add(new Vector3(1.0f, -1.0f, -1.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(1.0f, 0.0f, 0.0f));

        verts.Add(new Vector3(1.0f, 1.0f, 1.0f));
        verts.Add(new Vector3(-1.0f, 1.0f, 1.0f));
        verts.Add(new Vector3(-1.0f, 1.0f, -1.0f));
        verts.Add(new Vector3(1.0f, 1.0f, -1.0f));
        normals.Add(new Vector3(0.0f, 1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, 1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, 1.0f, 0.0f));
        normals.Add(new Vector3(0.0f, 1.0f, 0.0f));

        verts.Add(new Vector3(-1.0f, 1.0f, 1.0f));
        verts.Add(new Vector3(-1.0f, -1.0f, 1.0f));
        verts.Add(new Vector3(-1.0f, -1.0f, -1.0f));
        verts.Add(new Vector3(-1.0f, 1.0f, -1.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));
        normals.Add(new Vector3(-1.0f, 0.0f, 0.0f));

        LightingPipeline.boxSides.SetVertices(verts);

        LightingPipeline.boxSides.SetNormals(normals);

        int[] indices = new int[24];

        //Because the tris have to go clockwise about the Z-axis, these will all be facing backwards
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;

        indices[3] = 0;
        indices[4] = 2;
        indices[5] = 3;


        indices[6] = 4;
        indices[7] = 5;
        indices[8] = 6;

        indices[9] =  4;
        indices[10] = 6;
        indices[11] = 7;


        indices[12] = 8;
        indices[13] = 9;
        indices[14] = 10;

        indices[15] = 8;
        indices[16] = 10;
        indices[17] = 11;


        indices[18] = 12;
        indices[19] = 13;
        indices[20] = 14;

        indices[21] = 12;
        indices[22] = 14;
        indices[23] = 15;

        LightingPipeline.boxSides.SetIndices(indices, MeshTopology.Triangles, 0);

        LightingPipeline.boxSides.UploadMeshData(true);
        
    }

    protected override IRenderPipeline InternalCreatePipeline()
    {
        CreateQuad();

        CreateBoxSides();

        CreateLightMaterials();

        CreateShadowmap();

        return new LightingPipeline();
    }
}

public class LightingPipeline : RenderPipeline
{
    public static List<GBuffer> gbuffers = new List<GBuffer>();
    static RenderTextureFormat[] gbufferFormat = {
        RenderTextureFormat.ARGB32,     //Color
        RenderTextureFormat.ARGBFloat,  //Normal, Z-Position
        RenderTextureFormat.ARGB32,     //Metalness, Roughness
        RenderTextureFormat.ARGBHalf, //Emission + Light
    };

    //Quad from -1 to 1 on the XY plane
    public static Mesh quad;
    //Just the edges of a box. No planes facing forward, only X and Y
    public static Mesh boxSides;
    public static Material[] lightMaterials = new Material[3];
    public static Material[] occluderMaterials = new Material[3];
    public static RenderTexture shadowMap;
    public static int shmapResolution;
    public static int graphicsDebugMode = 0;
    public static float shadowBleedDistance = 0.5f;

    Matrix4x4[] GetLightArray(List<VisibleLight> lights, out Vector4[] lightColors, LightType type)
    {
        int numLights = 0;

        for (int i = 0; i < lights.Count; i++)
        {
            //if (lights[i].lightType == type)
            //    numLights++;
        }

        numLights = lights.Count;

        Matrix4x4[] o = new Matrix4x4[numLights];
        lightColors = new Vector4[numLights];

        int index = 0;

        for (int i = 0; i < lights.Count; i++)
        {
            index = i;
            //if (lights[i].lightType == type)
            {
                float radius = lights[i].range;
                o[index] = lights[i].localToWorld * Matrix4x4.Scale(new Vector3(radius, radius, radius));
                lightColors[index] = lights[i].finalColor;
            }
        }

        return o;
    }

    public override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        base.Render(context, cameras);

        int camIndex = 0;

        foreach (Camera c in cameras)
        {
            //First, GBuffer cheqque
            //First first, check the camera's target
            int w, h;

            if (c.targetTexture == null)
            {
                w = Screen.width;
                h = Screen.height;
            }
            else
            {
                w = c.targetTexture.width;
                h = c.targetTexture.height;
            }

            if (camIndex >= gbuffers.Count)
            {
                gbuffers.Add(new GBuffer(w, h, 24, gbufferFormat));
            }
            else if (w != gbuffers[camIndex].w || h != gbuffers[camIndex].h)
            {
                if (gbuffers[camIndex] != null)
                    gbuffers[camIndex].Release();

                gbuffers[camIndex] = new GBuffer(w, h, 24, gbufferFormat);
            }

            //This matrix converts special UV+Z vectors into world space
            Matrix4x4 uvzTOworld = new Matrix4x4(c.transform.right * c.aspect * c.orthographicSize, c.transform.up * c.orthographicSize, new Vector4(0.0f, 0.0f, 1.0f, 0.0f), new Vector4(c.transform.position.x, c.transform.position.y, 0.0f, 1.0f));

            GBuffer gbuf = gbuffers[camIndex];

            ScriptableCullingParameters culling;

            if (!CullResults.GetCullingParameters(c, out culling))
                continue;

            CullResults cull = CullResults.Cull(ref culling, context);

            context.SetupCameraProperties(c);

            CommandBuffer initGBuffer = new CommandBuffer();
            
            //initGBuffer.ClearRenderTarget(true, true, Color.black);

            //context.ExecuteCommandBuffer(clearDepth);
            //clearDepth.Release();
            initGBuffer.SetRenderTarget(gbuf.layerIdentifiers, gbuf.depthIdentifier);

            initGBuffer.ClearRenderTarget(true, true, Color.black);

            initGBuffer.SetGlobalTexture("_GBuffer0", gbuf.colorAttachments[0]);
            initGBuffer.SetGlobalTexture("_GBuffer1", gbuf.colorAttachments[1]);
            initGBuffer.SetGlobalTexture("_GBuffer2", gbuf.colorAttachments[2]);
            initGBuffer.SetGlobalTexture("_GBuffer3", gbuf.colorAttachments[3]);

            initGBuffer.SetGlobalMatrix("_UVZtoWORLD", uvzTOworld);

            context.ExecuteCommandBuffer(initGBuffer);

            initGBuffer.Release();

            //Actually render stuff now
            {
                Shader.SetGlobalVector("_AmbientColor", RenderSettings.ambientLight);

                DrawRendererSettings currentPass = new DrawRendererSettings(c, new ShaderPassName("DeferredSprite"));
                currentPass.sorting.flags = SortFlags.CommonOpaque;
            
                FilterRenderersSettings passFilter = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.opaque };
            
                context.DrawRenderers(cull.visibleRenderers, ref currentPass, passFilter);
            }

            //Debug stuff here
            if (graphicsDebugMode > 0)
            {
                CommandBuffer outputDebugInfo = new CommandBuffer();

                context.SetupCameraProperties(c);

                outputDebugInfo.Blit(gbuf.layerIdentifiers[graphicsDebugMode - 1], BuiltinRenderTextureType.CameraTarget);

                context.ExecuteCommandBuffer(outputDebugInfo);

                outputDebugInfo.Release();

                context.Submit();

                return;
            }

            //Now render all shadowmaps
            if (cull.visibleLights.Count > 0)
            {
                CommandBuffer clearShadowmapBuffer = new CommandBuffer();

                clearShadowmapBuffer.SetRenderTarget(shadowMap);
                clearShadowmapBuffer.ClearRenderTarget(true, true, new Color(100.0f, 100.0f, 100.0f, 100.0f), 1.0f);
                clearShadowmapBuffer.SetGlobalFloat("_ShadowBleedDistance", shadowBleedDistance);
                
                int light = 0;
                
                foreach (VisibleLight L in cull.visibleLights)
                {
                    SpriteLight sl = L.light.GetComponent<SpriteLight>();
                    if (sl != null)
                    {
                        Matrix4x4[] occluderMatrices = new Matrix4x4[sl.boxOccluders.Count];

                        //occluderMaterials[0].SetFloat("_Radius", L.light.range);
                        //occluderMaterials[0].SetFloat("_Column", (float)light);
                        //occluderMaterials[0].SetVector("_lightPosition", L.light.transform.position);
                        //occluderMaterials[0].SetVector("_TexelSize", new Vector4(1.0f / shmapResolution, 1.0f / shmapResolution, shmapResolution, shmapResolution));

                        clearShadowmapBuffer.SetGlobalFloat("_Radius", L.light.range);
                        clearShadowmapBuffer.SetGlobalFloat("_Column", (float)light);
                        clearShadowmapBuffer.SetGlobalVector("_lightPosition", L.light.transform.position);
                        clearShadowmapBuffer.SetGlobalVector("_TexelSize", new Vector4(1.0f / shmapResolution, 1.0f / shmapResolution, shmapResolution, shmapResolution));

                        int i = 0;
                        foreach (SpriteShadowCaster ren in sl.boxOccluders)
                        {
                            occluderMatrices[i++] = ren.transform.localToWorldMatrix;
                        }

                        //MonoBehaviour.print(shadowMap.width);
                        
                        clearShadowmapBuffer.DrawMeshInstanced(boxSides, 0, occluderMaterials[0], 0, occluderMatrices);

                        //Now, draw all meshes that're occluding and stuff
                        foreach (SpriteShadowCaster ren in sl.meshOccluders)
                        {
                            MeshFilter mr = ren.GetComponent<MeshFilter>();
                            clearShadowmapBuffer.DrawMesh(mr.sharedMesh, mr.transform.localToWorldMatrix, occluderMaterials[1]);
                            //clearShadowmapBuffer.DrawMesh(boxSides, mr.transform.localToWorldMatrix, occluderMaterials[1]);
                        }
                    }
                    light++;
                }

                context.ExecuteCommandBuffer(clearShadowmapBuffer);

                clearShadowmapBuffer.Clear();
            }

            //Now, render the skybox into the light buffer
            {

                CommandBuffer beginLightRender = new CommandBuffer();

                beginLightRender.SetRenderTarget(gbuf.layerIdentifiers[3], gbuf.depthIdentifier);

                context.ExecuteCommandBuffer(beginLightRender);

                beginLightRender.Release();

                context.DrawSkybox(c);
            }

            //context.
            
            //context.SetupCameraProperties(c);

            //Render all lights
            if (cull.visibleLights.Count > 0)
            {
                CommandBuffer drawLights = new CommandBuffer();
                Vector4[] lightColors;

                Matrix4x4[] lightArray = GetLightArray(cull.visibleLights, out lightColors, LightType.Point);
                
                //if (c.name == "SceneCamera" || c.name == "Preview Camera")
                //    lightMaterials[0].SetFloat("_FlipY", 1.0f);
                //else
                //    lightMaterials[0].SetFloat("_FlipY", 0.0f);

                lightMaterials[0].SetTexture("_MainTex", shadowMap);
                lightMaterials[0].SetVectorArray("_LightColor", lightColors);

                drawLights.DrawMeshInstanced(quad, 0, lightMaterials[0], 0, lightArray, lightArray.Length);
                //drawLights.DrawMesh(quad, lightArray[0], lightMaterials[0]);

                context.ExecuteCommandBuffer(drawLights);

                drawLights.Release();
            }

            //cull.visibleLights[0].
            
            {
                DrawRendererSettings currentPass = new DrawRendererSettings(c, new ShaderPassName("Forward Transparent"));
                currentPass.sorting.flags = SortFlags.CommonOpaque;

                FilterRenderersSettings passFilter = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.opaque };

                context.DrawRenderers(cull.visibleRenderers, ref currentPass, passFilter);
            }

            //Default object rendering (won't be affected by lighting)
            {
                DrawRendererSettings currentPass = new DrawRendererSettings(c, new ShaderPassName("SRPDefaultUnlit"));
                currentPass.sorting.flags = SortFlags.CommonOpaque;
                //currentPass.SetShaderPassName(1, new ShaderPassName("ForwardAdd"));

                FilterRenderersSettings passFilter = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.all };
                
                context.DrawRenderers(cull.visibleRenderers, ref currentPass, passFilter);
            }

            //{
            //    DrawRendererSettings currentPass = new DrawRendererSettings(c, new ShaderPassName("SRPDefaultUnlit"));
            //    currentPass.sorting.flags = SortFlags.CommonOpaque;
            //    //currentPass.SetShaderPassName(1, new ShaderPassName("ForwardAdd"));
            //
            //    FilterRenderersSettings passFilter = new FilterRenderersSettings(true) { renderQueueRange = RenderQueueRange.all };
            //
            //    //context.DrawRenderers(, ref currentPass, passFilter);
            //}

            //Blit final light to screen
            {
                context.SetupCameraProperties(c);

                CommandBuffer blitImage = new CommandBuffer();

                blitImage.Blit(gbuf.colorAttachments[3], BuiltinRenderTextureType.CameraTarget);

                context.ExecuteCommandBuffer(blitImage);

                blitImage.Release();
            }

            //CommandBuffer BlitStencil = new CommandBuffer();
            //
            //BlitStencil.Blit(gbuffers[camIndex].depthAttachment, BuiltinRenderTextureType.CameraTarget, );
            //
            //context.ExecuteCommandBuffer(buf);
            //
            //buf.Release();

            context.Submit();
            camIndex++;
        }
    }
}