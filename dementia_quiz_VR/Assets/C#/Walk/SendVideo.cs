using UnityEngine;
using System;
using System.Collections;
using System.IO;
using WebSocketSharp;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class ScreenShareSender : MonoBehaviour
{
    [SerializeField]
    StatusData statusData;
    public int captureWidth = 256;
    public int captureHeight = 144;
    public int jpegQuality = 100;
    [SerializeField]
    private Camera mainCamera;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private WebSocket ws;

    void Start()
    {
        InitializeWebSocket();
        InitializeCapture();
    }

    private void InitializeWebSocket()
    {
        string serverUrl = $"wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/cast/unity/{statusData.UUID}";
        ws = new WebSocket(serverUrl);
        ws.OnOpen += (sender, e) => UnityEngine.Debug.Log("WebSocket connected");
        ws.OnError += (sender, e) => UnityEngine.Debug.LogError("WebSocket error: " + e.Message);
        ws.OnClose += (sender, e) => UnityEngine.Debug.Log($"WebSocket closed: {e.Reason}");
        ws.Connect();
    }

    private void InitializeCapture()
    {
        mainCamera = Camera.main;
        renderTexture = new RenderTexture(captureWidth, captureHeight, 16, RenderTextureFormat.ARGB32);
        texture2D = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
    }

    void Update()
    {
        StartCoroutine(CaptureAndSendFrame());
    }

    private IEnumerator CaptureAndSendFrame()
    {
        yield return new WaitForEndOfFrame();

        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        texture2D.Apply();

        byte[] jpegData = texture2D.EncodeToJPG();
        SendImage(jpegData);

        mainCamera.targetTexture = null;
        RenderTexture.active = null;
    }

    private void SendImage(byte[] encodedImage)
    {
        try
        {
            string base64Image = Convert.ToBase64String(encodedImage);
            ws.Send("{\"data\":\"" + base64Image + "\"}");
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"WebSocket send error: {e.Message}");
        }
    }

    private void OnDestroy()
    {
        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }

        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
        if (texture2D != null)
        {
            Destroy(texture2D);
        }
    }
}