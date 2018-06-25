using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Light))]
public class SpriteLight : MonoBehaviour
{
    SphereCollider col;
    Light l;

    public List<SpriteShadowCaster> boxOccluders = new List<SpriteShadowCaster>();
    public List<SpriteShadowCaster> meshOccluders = new List<SpriteShadowCaster>();

    // Use this for initialization
    void OnEnable ()
    {
        col = GetComponent<SphereCollider>();
        l = GetComponent<Light>();
        col.isTrigger = true;
        boxOccluders.Clear();
        meshOccluders.Clear();

#if UNITY_EDITOR
        firstFrame = true;
#endif
    }

    private void OnTriggerEnter(Collider other)
    {
        SpriteShadowCaster ren = other.GetComponent<SpriteShadowCaster>();

        if (ren != null && ren.enabled)
        {
            switch (ren.shadowType)
            {
                case CasterType.Box:
                    boxOccluders.Add(ren);
                    break;
                case CasterType.Mesh:
                    meshOccluders.Add(ren);
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        SpriteShadowCaster ren = other.GetComponent<SpriteShadowCaster>();

        if (ren != null && ren.enabled)
        {
            switch (ren.shadowType)
            {
                case CasterType.Box:
                    boxOccluders.Remove(ren);
                    break;
                case CasterType.Mesh:
                    meshOccluders.Remove(ren);
                    break;
                default:
                    break;
            }
        }
    }
#if UNITY_EDITOR

    Vector3 oldPos = new Vector3();

    bool firstFrame = true;

    void GetAllNearbyObjects()
    {
        SpriteShadowCaster[] allSprites = Object.FindObjectsOfType<SpriteShadowCaster>();

        float lenSq = l.range * l.range;

        foreach (SpriteShadowCaster ren in allSprites)
        {
            Collider col = ren.GetComponent<Collider>();

            Vector3 difference = ren.transform.position - transform.position;
            difference.z = 0.0f;

            if (col != null && difference.sqrMagnitude <= lenSq)
            {
                switch (ren.shadowType)
                {
                    case CasterType.Box:
                        boxOccluders.Add(ren);
                        break;
                    case CasterType.Mesh:
                        meshOccluders.Add(ren);
                        break;
                    default:
                        break;
                }

                //boxOccluders.Add(ren);
            }
        }
    }

    private void Update()
    {
        col.radius = l.range;
        
        if (!EditorApplication.isPlaying && (oldPos != transform.localPosition || firstFrame))
        {
            firstFrame = false;
            boxOccluders.Clear();
            meshOccluders.Clear();
            GetAllNearbyObjects();
        }

        oldPos = transform.localPosition;
    }
#else
    private void Update()
    {
        col.radius = l.range;
    }
#endif
}
