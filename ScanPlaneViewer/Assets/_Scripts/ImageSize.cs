using System;
using UnityEngine;

public class ImageSize : MonoBehaviour
{
    public float width_pix, height_pix;
    public float x, y;


    void Start()
    {
        _Position();
        _Resize();
        //ImageDisplayer._instance._FocusOnTarget();
    }

    void _Position()
    {
        transform.position = new Vector3(x, 0, y);
    }

    public void _Resize()
    {
        transform.localScale = new Vector3(width_pix, 1f, height_pix) * ImageDisplayer._instance.facteur1 * ImageDisplayer._instance.facteur2;
        transform.rotation = Quaternion.Euler(0, (float)ImageDisplayer._instance._angles, 0);
    }

}
