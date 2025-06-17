using UnityEngine;
using System.Collections.Generic;
using System.Collections;
public class ScrollingBackGround : MonoBehaviour 
{
    public float speed;
    [SerializeField]
    private Renderer bgRenderer;
    private void Update()
    {
        bgRenderer.material.mainTextureOffset += new Vector2(0, speed * Time.deltaTime);
    }
}
