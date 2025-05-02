using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TexToImage : MonoBehaviour
{
    Image image;

    public void ApplyTex(Texture2D tex)
    {
        if (image == null)
            image = GetComponent<Image>();

        if (image != null && tex != null)
        {
            if(image.sprite != null)
            {
                Destroy(image.sprite);
                image.sprite = null;

                Debug.Log("Got image");
            }

            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            //image.SetNativeSize();
        }
    }
}
