using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageDisplayer : MonoBehaviour
{
    public bool focus;

    bool? needtoreplace_dot_by_comma = null;

    public float facteur1 = 0.001f;
    public float facteur2 = 1f;

    public enum Angles { _0 = 0, _90 = 90, _180 = 180, _270 = 270 }
    public Angles _angles;

    bool firstautoscale = true;

    List<ImageSize> imagesSize = new List<ImageSize>();

    public static ImageDisplayer _instance;
    void Awake()
    {
        _instance = this;
    }

    public void _OnNewImageFile(string imagefilename)
    {
        LoadImage(imagefilename);
        if (focus)
            _FocusOnTarget();
    }

    void LoadImage(string imagefilename)
    {
        if (File.Exists(imagefilename))
        {
            // Charge l'image depuis le disque dur
            byte[] fileData = File.ReadAllBytes(imagefilename);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData); // Charge l'image dans la texture
            try
            {
                // Crée un nouveau matériau avec un shader approprié
                Material material = new Material(Shader.Find("Unlit/Texture"));
                material.mainTexture = texture;

                // Crée un nouveau Plane
                GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
                // Applique le matériau au Plane
                plane.GetComponent<Renderer>().material = material;

                //set Parent
                plane.transform.parent = transform;

                ImageSize imageSize = plane.AddComponent<ImageSize>();
                imageSize.width_pix = texture.width;
                imageSize.height_pix = texture.height;

                if (firstautoscale)
                {
                    firstautoscale = false;
                    facteur2 = 1000 / imageSize.height_pix;
                }

                //nom -> coordonnées
                FileInfo fi = new FileInfo(imagefilename);
                string nom = fi.Name.Substring(0, fi.Name.Length - fi.Extension.Length); // 0.123;0.005
                string[] xy_string = nom.Split(';');

                if (needtoreplace_dot_by_comma == null)
                    needtoreplace_dot_by_comma = !float.TryParse(xy_string[0], out float i);

                if (needtoreplace_dot_by_comma == true)
                {
                    xy_string[0] = xy_string[0].Replace('.', ',');
                    xy_string[1] = xy_string[1].Replace('.', ',');
                }
                plane.name = nom;

                imageSize.x = float.Parse(xy_string[0]);
                imageSize.y = float.Parse(xy_string[1]);

                imagesSize.Add(imageSize);

            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        else
        {
            Debug.LogError("Le fichier image n'existe pas : " + imagefilename);
        }
    }

    void OnValidate()
    {
        foreach (var imagesize in imagesSize)
            imagesize._Resize();
    }

    void Update()
    {
        if (focus)
        {
            focus = false;
            _FocusOnTarget();
        }
    }

    public void _FocusOnTarget()
    {
        //FocusOnTarget(Camera.main, ImageDisplayer._instance.gameObject);
        FitCameraAbove(Camera.main, ImageDisplayer._instance.gameObject);
    }
    void FitCameraAbove(Camera targetCamera, GameObject targetObject, Vector3? offsetDirection = null)
    {
        Bounds bounds = GetTotalBounds(targetObject);
        Vector3 center = bounds.center;
        float padding = 1.1f; // Pour que ce soit un peu plus large que l'objet

        // Taille à couvrir sur XZ
        float width = bounds.size.x * padding;
        float depth = bounds.size.z * padding;

        // Taille maximale à couvrir selon l’aspect ratio
        float maxHorizontal = Mathf.Max(width, depth / targetCamera.aspect);

        // Calcul de la hauteur (Y) nécessaire avec le FOV vertical
        float fovRad = targetCamera.fieldOfView * Mathf.Deg2Rad;
        float distance = (maxHorizontal / 2f) / Mathf.Tan(fovRad / 2f);

        // Positionne la caméra au-dessus et regarde vers le bas
        Vector3 cameraPosition = new Vector3(center.x, center.y + distance, center.z);
        targetCamera.transform.position = cameraPosition;
        targetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Regard vertical vers le bas
    }

    void FocusOnTarget(Camera targetCamera, GameObject targetObject, Vector3? offsetDirection = null)
    {
        Bounds bounds = GetTotalBounds(targetObject);
        Vector3 center = bounds.center;
        float padding = 1.1f; // Pour que ce soit un peu plus large que l'objet

        // Position de la caméra : directement au-dessus (axe Y+), regardant vers le bas
        float cameraHeight = 10f; // Valeur temporaire pour placer la caméra

        targetCamera.transform.position = new Vector3(center.x, cameraHeight, center.z);
        targetCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f); // Regard vers -Y

        // Assure-toi que la caméra est orthographique
        targetCamera.orthographic = true;

        // Calcul de la taille nécessaire
        float width = bounds.size.x * padding;
        float height = bounds.size.z * padding;

        // On prend la moitié de la taille la plus grande en tenant compte du ratio écran
        float screenAspect = (float)Screen.width / Screen.height;
        float cameraSize = Mathf.Max(height / 2f, width / (2f * screenAspect));

        targetCamera.orthographicSize = cameraSize;
    }

    Bounds GetTotalBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(go.transform.position, Vector3.zero);

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer rend in renderers)
            bounds.Encapsulate(rend.bounds);

        return bounds;
    }

}
