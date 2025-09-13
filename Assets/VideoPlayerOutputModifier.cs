using System;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

public class VideoPlayerOutputModifier : MonoBehaviour
{
    private VideoPlayer _videoPlayer;
    private MeshRenderer _meshRenderer;
    private Material _material;

    private void Start()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
        _meshRenderer = GetComponent<MeshRenderer>();
        _material = _meshRenderer.materials[0];
        _videoPlayer.targetTexture = new RenderTexture((int)_videoPlayer.width, (int)_videoPlayer.height, 0, RenderTextureFormat.ARGB32)
            {
                wrapMode = TextureWrapMode.Repeat
            };
        _videoPlayer.targetTexture.Create();
        _material.SetTexture("_BaseMap", _videoPlayer.targetTexture);
    }

    private void OnDestroy()
    {
        _videoPlayer.targetTexture.Release();
    }
}
