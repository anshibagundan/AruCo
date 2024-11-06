using UnityEngine;
using System;
using System.Collections;
using WebSocketSharp;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using App.BaseSystem.DataStores.ScriptableObjects.Status;

public class SendVideo : MonoBehaviour
{
    private WebSocket ws;
    public string serverUrl = $"wss://hopcardapi-4f6e9a3bf06d.herokuapp.com/ws/cast/unity/";
    [Header("Capture Settings")]
    public float captureFrameRate = 3f;        // 1�b������3�t���[���ɒጸ
    public int captureWidth = 256;             // ��菬�����𑜓x
    public int captureHeight = 144;            // 16:9�A�X�y�N�g��ێ�
    public int jpegQuality = 25;               // ��苭�����k

    [Header("Optimization")]
    public float pixelChangeTolerance = 0.15f; // �������o��臒l���グ��
    public int samplingRate = 4;               // �s�N�Z����r�̊Ԉ�����

    private Camera mainCamera;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private byte[] lastFrameData;
    [SerializeField]
    public StatusData statusData;

    // �L���v�`������p
    private float captureInterval;
    private float lastCaptureTime;
    private bool isProcessingFrame = false;
    private readonly object frameLock = new object();

    void Start()
    {
        Application.targetFrameRate = 30; // �t���[�����[�g����
        QualitySettings.vSyncCount = 0;   // VSync���I�t

        captureInterval = 1f / captureFrameRate;
        InitializeWebSocket();
        InitializeCapture();

        // �������g�p�ʂ��ŏ�����
        texture2D = new Texture2D(captureWidth, captureHeight, TextureFormat.RGB24, false);
        texture2D.Compress(true);
    }

    private void InitializeWebSocket()
    {
        if (statusData == null)
        {
            Debug.LogError("StatusData is not assigned!");
            return;
        }

        if (string.IsNullOrEmpty(statusData.UUID))
        {
            Debug.LogError("UUID is null or empty!");
            return;
        }

        string fullUrl = $"{serverUrl.TrimEnd('/')}/{statusData.UUID}";
        ws = new WebSocket(fullUrl);
        ws.SslConfiguration.ServerCertificateValidationCallback = OnCertificateValidation;
        ws.Log.Level = LogLevel.Error;

        // WebSocket�̃o�b�t�@�T�C�Y���œK��
        ws.WaitTime = TimeSpan.FromSeconds(2);

        ws.OnOpen += (sender, e) => Debug.Log("WebSocket connected");
        ws.OnError += (sender, e) => Debug.LogError("WebSocket error: " + e.Message);
        ws.OnClose += (sender, e) => Debug.Log($"WebSocket closed: {e.Reason}");

        ws.Connect();
    }

    private void InitializeCapture()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found!");
            enabled = false;
            return;
        }

        renderTexture = new RenderTexture(captureWidth, captureHeight, 16, RenderTextureFormat.RGB565); // ���F
        renderTexture.antiAliasing = 1; // �A���`�G�C���A�X�𖳌���
        renderTexture.Create();
    }

    void Update()
    {
        if (!ws.IsAlive || Time.time - lastCaptureTime < captureInterval || isProcessingFrame) return;
        StartCoroutine(CaptureAndSendFrameCoroutine());
    }

    private IEnumerator CaptureAndSendFrameCoroutine()
    {
        lock (frameLock)
        {
            if (isProcessingFrame) yield break;
            isProcessingFrame = true;
        }

        lastCaptureTime = Time.time;

        if (mainCamera == null)
        {
            isProcessingFrame = false;
            yield break;
        }

        // �t���[���̏I���܂ő҂�
        yield return new WaitForEndOfFrame();

        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();
        RenderTexture.active = renderTexture;

        texture2D.ReadPixels(new Rect(0, 0, captureWidth, captureHeight), 0, 0);
        texture2D.Apply(false); // mipmap���X�V���Ȃ�

        mainCamera.targetTexture = null;
        RenderTexture.active = null;

        byte[] currentFrameData = texture2D.EncodeToJPG(jpegQuality);

        if (ShouldSendFrame(currentFrameData))
        {
            try
            {
                string base64Image = Convert.ToBase64String(currentFrameData);
                ws.Send("{\"data\":\"" + base64Image + "\"}");
                lastFrameData = currentFrameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"WebSocket send error: {e.Message}");
            }
        }

        isProcessingFrame = false;
    }

    private bool ShouldSendFrame(byte[] currentFrame)
    {
        if (lastFrameData == null || lastFrameData.Length != currentFrame.Length) return true;

        int differentBytes = 0;
        int totalBytes = currentFrame.Length;

        // �T���v�����O���[�g�Ɋ�Â��ĊԈ����Ȃ����r
        for (int i = 0; i < totalBytes; i += samplingRate)
        {
            if (lastFrameData[i] != currentFrame[i])
            {
                differentBytes += samplingRate; // �T���v�����O���[�g���J�E���g
                if ((float)differentBytes / totalBytes > pixelChangeTolerance)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool OnCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        return true;
    }

    void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }

        if (texture2D != null)
        {
            Destroy(texture2D);
        }

        if (ws != null && ws.IsAlive)
        {
            ws.Close();
        }
    }
}