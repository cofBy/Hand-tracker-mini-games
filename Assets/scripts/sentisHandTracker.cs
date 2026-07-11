using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class sentisHandTracker : MonoBehaviour
{
    [Header("Detecting Palms")]
    public ModelAsset palmModelAsset;
    Model runtimePalmModel;
    Worker palmWorker;
    Tensor<float> palmInputTensor;

    [Range(0f, 1f)] public float scoreThreshold;
    [Range(0f, 1f)] public float iouThreshold;

    Rect handRect = Rect.zero;
    List<Vector2> anchors = new List<Vector2>();
    TextureTransform nhwcTransform;

    [Header("Tracking Hands")]
    public ModelAsset model;
    Model runtimeModel;
    Worker worker;
    Tensor<float> inputTensor;

    [Header("Camera Feed")]
    WebCamTexture webCam;
    RenderTexture croppedHandBuffer;

    [Header("visualize hand")]
    public Transform joint;
    public Canvas canvas;
    List<Transform> jointInstances = new List<Transform>();

    public LineRenderer fingerPrefap;
    List<LineRenderer> fingers = new List<LineRenderer>();

    [Header("debuging")]
    public RectTransform debugBox;
    public RawImage croppedPalm;

    private void Start()
    {
        for (int i = 0; i < 21; i++)
        {
            Transform jointInstance = Instantiate(joint, transform);
            jointInstances.Add(jointInstance);

            if (i < 5)
            {
                LineRenderer lineInstance = Instantiate(fingerPrefap, transform);
                lineInstance.positionCount = fingerJoints[i].Length;
                fingers.Add(lineInstance);
            }
        }

        runtimePalmModel = ModelLoader.Load(palmModelAsset);
        palmWorker = new Worker(runtimePalmModel, BackendType.GPUCompute);
        palmInputTensor = new Tensor<float>(new TensorShape(1, 192, 192, 3));
        nhwcTransform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC);

        anchors = GeneratePalmAnchors();

        runtimeModel = ModelLoader.Load(model);
        worker = new Worker(runtimeModel, BackendType.GPUCompute);
        inputTensor = new Tensor<float>(new TensorShape(1, 224, 224, 3));

        croppedHandBuffer = new RenderTexture(224, 224, 0, RenderTextureFormat.ARGB32);
        croppedPalm.texture = croppedHandBuffer;

        webCam = new WebCamTexture();
        webCam.Play();
    }
    List<Vector2> GeneratePalmAnchors()
    {
        var anchors = new List<Vector2>();
        int[] strides = { 8, 16, 16, 16 };
        const int inputSize = 192;
        const int anchorsPerCell = 2;

        foreach (int stride in strides)
        {
            int featureMapSize = inputSize / stride;
            for (int y = 0; y < featureMapSize; y++)
                for (int x = 0; x < featureMapSize; x++)
                {
                    float xCenter = (x + 0.5f) / featureMapSize;
                    float yCenter = (y + 0.5f) / featureMapSize;
                    for (int a = 0; a < anchorsPerCell; a++)
                        anchors.Add(new Vector2(xCenter, yCenter));
                }
        }
        return anchors;
    }
    private void Update()
    {
        if (webCam == null || !webCam.didUpdateThisFrame) return;

        handRect = ExpandToSquare(PerformPalmDetection(), 3f);

        if (handRect.width > 0 && handRect.height > 0)
        {
            CropHandRegion(webCam, handRect);

            TextureConverter.ToTensor(croppedHandBuffer, inputTensor, nhwcTransform);

            worker.Schedule(inputTensor);

            if (worker.PeekOutput() is Tensor<float> outputTensor)
            {
                using Tensor<float> cpuTensor = outputTensor.ReadbackAndClone();
                ProcessLandmarks(cpuTensor, handRect, out Vector2[] landMarks);

                if (landMarks != null)
                {
                    for (int i = 0; i < landMarks.Length; i++) 
                    {
                        jointInstances[i].gameObject.SetActive(true);
                        jointInstances[i].position = landMarks[i];
                    }

                    for (int f = 0; f < fingers.Count; f++)
                    {
                        fingers[f].gameObject.SetActive(true);
                        for (int p = 0; p < fingerJoints[f].Length; p++)
                        {
                            int landmarkIndex = fingerJoints[f][p];
                            fingers[f].SetPosition(p, landMarks[landmarkIndex]);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < jointInstances.Count; i++)
                    {
                        jointInstances[i].gameObject.SetActive(false);
                        if (i < fingers.Count)
                        {
                            fingers[i].gameObject.SetActive(false);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < jointInstances.Count; i++)
                {
                    jointInstances[i].gameObject.SetActive(false);
                    if (i < fingers.Count)
                    {
                        fingers[i].gameObject.SetActive(false);
                    }
                }
            }

            float screenX = (handRect.x + handRect.width * 0.5f) * Screen.width;
            float screenY = (1f - (handRect.y + handRect.height * 0.5f)) * Screen.height;
            debugBox.position = new Vector3(screenX, screenY, 0);
            debugBox.sizeDelta = new Vector2(handRect.width * Screen.width, handRect.height * Screen.height);
        }
    }

    int[][] fingerJoints = new int[][]
    {
        new int[] {0,1,2,3,4},    // thumb
        new int[] {0,5,6,7,8},    // index
        new int[] {0,9,10,11,12}, // middle
        new int[] {0,13,14,15,16},// ring
        new int[] {0,17,18,19,20} // pinky
    };
    private Rect ExpandToSquare(Rect box, float scale)
    {
        float cx = box.x + box.width * 0.5f;
        float cy = box.y + box.height * 0.5f;
        float size = Mathf.Max(box.width, box.height) * scale;

        float x = Mathf.Clamp(cx - size * 0.5f, 0, 1 - size);
        float y = Mathf.Clamp(cy - size * 0.5f, 0, 1 - size);
        return new Rect(x, y, size, size);
    }
    private Rect PerformPalmDetection()
    {
        TextureConverter.ToTensor(webCam, palmInputTensor, nhwcTransform);
        palmWorker.Schedule(palmInputTensor);

        Tensor<float> rawBoxes = palmWorker.PeekOutput(0) as Tensor<float>;
        Tensor<float> rawScores = palmWorker.PeekOutput(1) as Tensor<float>;

        if (rawBoxes == null || rawScores == null) return Rect.zero;

        Tensor<float> boxes = rawBoxes;
        Tensor<float> scores = rawScores;

        if (boxes.count == 2016 && scores.count == 36288)
        {
            rawBoxes = scores;
            rawScores = boxes;
        }

        float[] boxData = rawBoxes.DownloadToArray();
        float[] scoreData = rawScores.DownloadToArray();

        return ParsePalmBBoxes(boxData, scoreData);
    }
    private Rect ParsePalmBBoxes(float[] boxData, float[] scoreData)
    {
        int bestIndex = 0;
        float maxScore = float.MinValue;
        for (int i = 0; i < scoreData.Length; i++)
        {
            if (scoreData[i] > maxScore)
            {
                maxScore = scoreData[i]; bestIndex = i;
            }
        }

        float confidence = 1f / (1f + Mathf.Exp(-maxScore));
        if (confidence < scoreThreshold) return Rect.zero;

        int offset = bestIndex * 18;
        Vector2 anchor = anchors[bestIndex];
        const float inputSize = 192f;

        float cx = boxData[offset + 0] / inputSize + anchor.x;
        float cy = boxData[offset + 1] / inputSize + anchor.y;
        float w = boxData[offset + 2] / inputSize;
        float h = boxData[offset + 3] / inputSize;

        float xMin = Mathf.Clamp01(cx - w * 0.5f);
        float yMin = Mathf.Clamp01(cy - h * 0.5f);
        return new Rect(xMin, yMin, w, h);
    }
    private void CropHandRegion(Texture sourceTex, Rect area)
    {
        Vector2 scale = new Vector2(area.width, area.height);
        Vector2 offset = new Vector2(area.x, 1f - area.y - area.height);
        Graphics.Blit(sourceTex, croppedHandBuffer, scale, offset);
    }
    void ProcessLandmarks(Tensor<float> tensorData, Rect appliedCrop, out Vector2[]worldLandMarks)
    {
        if (tensorData.count >= 63)
        {
            float[] data = tensorData.DownloadToArray();

            Vector2[] worldPositions = new Vector2[21];
            for (int i = 0; i < 63; i += 3)
            {
                float cropX = data[i] / 224f;
                float cropY = data[i + 1] / 224f;
                float cropZ = data[i + 2] / 224f;

                float screenX = (appliedCrop.x + (cropX * appliedCrop.width)) * Screen.width;
                float screenY = (1.0f - (appliedCrop.y + (cropY * appliedCrop.height))) * Screen.height;
                Vector2 screenPos = new Vector3(screenX, screenY, 0);

                worldPositions[i / 3] = Camera.main.ScreenToWorldPoint(screenPos);
            }
            worldLandMarks = worldPositions;
        }
        else
        {
            worldLandMarks = null;
        }
    }

    private void OnDestroy()
    {
        palmWorker?.Dispose();
        worker?.Dispose();
        palmInputTensor?.Dispose();
        inputTensor?.Dispose();

        if (croppedHandBuffer != null)
        {
            croppedHandBuffer.Release();
            Destroy(croppedHandBuffer);
        }
    }
}