using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class sentisHandTracker : MonoBehaviour
{
    [Header("Detecting Palms")]
    public ModelAsset palmModelAsset;
    Model runtimePalmModel;
    Worker palmWorker;
    Tensor<float> palmInputTensor;

    [Range(0f, 1f)] public float scoreThreshold = 0.5f;
    [Range(0f, 1f)] public float iouThreshold = 0.3f;

    [Header("Multi-hand")]
    [Range(1, 4)] public int maxHands = 2;

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
    Material blitMaterial;

    [Header("Visualize Hand")]
    public Transform joint;
    public Canvas canvas;
    public LineRenderer fingerPrefap;

    List<List<Transform>> jointInstancesPerHand = new List<List<Transform>>();
    List<List<LineRenderer>> fingersPerHand = new List<List<LineRenderer>>();

    [Header("info")]
    public Vector2[] handLandmarks = new Vector2[21];

    [Header("Debugging")]
    public RectTransform debugBox;
    List<RectTransform> debugBoxesPerHand = new List<RectTransform>();
    List<TextMeshProUGUI> directionText = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> fistText = new List<TextMeshProUGUI>();
    public RawImage croppedPalm;
    public RawImage cameraFeed;
    public bool flipCamera;

    int[][] fingerJoints = new int[][]
    {
        new int[] {0,1,2,3,4},    // thumb
        new int[] {0,5,6,7,8},    // index
        new int[] {0,9,10,11,12}, // middle
        new int[] {0,13,14,15,16},// ring
        new int[] {0,17,18,19,20} // pinky
    };
    struct Candidate
    {
        public Rect box;
        public float score;
        public Vector2[] landMarks;
        public float angle;
    }

    private void Start()
    {
        Shader blitCopy = Shader.Find("Hidden/BlitCopy");
        Debug.Log(blitCopy);
        blitMaterial = new Material(blitCopy);

        WebCamDevice? frontCam = null;
        foreach (WebCamDevice d in WebCamTexture.devices)
        {
            if (d.isFrontFacing)
            {
                frontCam = d;
                break;
            }
        }
        WebCamDevice device = frontCam ?? WebCamTexture.devices[0];
        webCam = new WebCamTexture(device.name, 320, 240, 15);
        webCam.Play();
        flipCamera = device.isFrontFacing;

        if (cameraFeed != null)
        {
            cameraFeed.texture = webCam;
            if (flipCamera) cameraFeed.uvRect = new Rect(1, 0, -1, 1);
        }

        for (int h = 0; h < maxHands; h++)
        {
            var joints = new List<Transform>();
            for (int i = 0; i < 21; i++)
            {
                Transform jointInstance = Instantiate(joint, transform);
                jointInstance.gameObject.SetActive(false);
                joints.Add(jointInstance);
            }
            jointInstancesPerHand.Add(joints);

            var fingerSet = new List<LineRenderer>();
            for (int f = 0; f < 5; f++)
            {
                LineRenderer lineInstance = Instantiate(fingerPrefap, transform);
                lineInstance.positionCount = fingerJoints[f].Length;
                lineInstance.gameObject.SetActive(false);
                fingerSet.Add(lineInstance);
            }
            fingersPerHand.Add(fingerSet);

            if (debugBox != null)
            {
                RectTransform dbInstance = Instantiate(debugBox, canvas.transform);
                dbInstance.gameObject.SetActive(false);
                debugBoxesPerHand.Add(dbInstance);
                directionText.Add(dbInstance.GetChild(0).GetComponent<TextMeshProUGUI>());
                fistText.Add(dbInstance.GetChild(1).GetComponent<TextMeshProUGUI>());
            }
        }

        runtimePalmModel = ModelLoader.Load(palmModelAsset);
        palmWorker = new Worker(runtimePalmModel, BackendType.CPU);
        palmInputTensor = new Tensor<float>(new TensorShape(1, 192, 192, 3));
        nhwcTransform = new TextureTransform().SetTensorLayout(TensorLayout.NHWC);

        anchors = GeneratePalmAnchors();

        runtimeModel = ModelLoader.Load(model);
        worker = new Worker(runtimeModel, BackendType.CPU);
        inputTensor = new Tensor<float>(new TensorShape(1, 224, 224, 3));

        croppedHandBuffer = new RenderTexture(224, 224, 0, RenderTextureFormat.ARGB32);
        if (croppedPalm != null) croppedPalm.texture = croppedHandBuffer;
    }
    List<Vector2> GeneratePalmAnchors()
    {
        var anchorsList = new List<Vector2>();
        int[] strides = { 8, 16, 16, 16 };
        const int inputSize = 192;
        const int anchorsPerCell = 2;

        foreach (int stride in strides)
        {
            int featureMapSize = inputSize / stride;
            for (int y = 0; y < featureMapSize; y++)
            {
                for (int x = 0; x < featureMapSize; x++)
                {
                    float xCenter = (x + 0.5f) / featureMapSize;
                    float yCenter = (y + 0.5f) / featureMapSize;
                    for (int a = 0; a < anchorsPerCell; a++)
                    {
                        anchorsList.Add(new Vector2(xCenter, yCenter));
                    }
                }
            }
        }
        return anchorsList;
    }
    private void Update()
    {
        if (webCam == null || !webCam.didUpdateThisFrame) return;

        List<Candidate> handCandidates = PerformPalmDetection();

        for (int h = 0; h < maxHands; h++)
        {
            if (h >= handCandidates.Count)
            {
                setHandActive(h, false);
                continue;
            }

            Candidate candidate = handCandidates[h];
            Rect handRect = ExpandToSquare(candidate.box, 3f);

            if (handRect.width <= 0 || handRect.height <= 0)
            {
                setHandActive(h, false);
                continue;
            }

            CropHandRegion(webCam, handRect, candidate.angle);
            TextureConverter.ToTensor(croppedHandBuffer, inputTensor, nhwcTransform);
            worker.Schedule(inputTensor);

            if (worker.PeekOutput(0) is Tensor<float> outputTensor)
            {
                using Tensor<float> cpuTensor = outputTensor.ReadbackAndClone();
                ProcessLandmarks(cpuTensor, handRect, candidate.angle);
            }

            if (debugBox != null && worker.PeekOutput(2) is Tensor<float> direction)
            {
                using Tensor<float> directionCpu = direction.ReadbackAndClone();
                float isRightHand = directionCpu.DownloadToArray()[0];
                directionText[h].text = isRightHand < 0.5f ? "Left Hand" : "Right Hand";
            }

            if (handLandmarks != null)
            {
                setHandActive(h, true);

                List<Transform> joints = jointInstancesPerHand[h];
                List<LineRenderer> fingers = fingersPerHand[h];

                for (int i = 0; i < handLandmarks.Length && i < joints.Count; i++) joints[i].position = handLandmarks[i];

                for (int f = 0; f < fingers.Count; f++)
                {
                    for (int p = 0; p < fingerJoints[f].Length; p++)
                    {
                        int landmarkIndex = fingerJoints[f][p];
                        fingers[f].SetPosition(p, handLandmarks[landmarkIndex]);
                    }
                }

                int size = Mathf.Max(Screen.width, Screen.height);
                float offsetX = (Screen.width - size) / 2f;
                float offsetY = (Screen.height - size) / 2f;
                float screenX = (handRect.x + handRect.width * 0.5f) * size + offsetX;
                float screenY = (1f - (handRect.y + handRect.height * 0.5f)) * size + offsetY;

                if (flipCamera) screenX = Screen.width - screenX;

                if (debugBox != null)
                {
                    debugBoxesPerHand[h].gameObject.SetActive(true);
                    debugBoxesPerHand[h].position = new Vector3(screenX, screenY, 0);
                    debugBoxesPerHand[h].sizeDelta = new Vector2(handRect.width * size, handRect.height * size);

                    float angleDegrees = candidate.angle * Mathf.Rad2Deg;
                    debugBoxesPerHand[h].localRotation = Quaternion.Euler(0, 0, angleDegrees);
                }
            }
            else
            {
                setHandActive(h, false);
            }
        }
    }

    void setHandActive(int handIndex, bool active)
    {
        foreach (var j in jointInstancesPerHand[handIndex]) j.gameObject.SetActive(active);
        foreach (var f in fingersPerHand[handIndex]) f.gameObject.SetActive(active);
        if (!active && debugBox != null) debugBoxesPerHand[handIndex].gameObject.SetActive(false);
    }

    Rect ExpandToSquare(Rect box, float scale)
    {
        float w = Mathf.Clamp(box.width, 0, 1 / scale);
        float h = Mathf.Clamp(box.height, 0, 1 / scale);

        float cx = box.x + w * 0.5f;
        float cy = box.y + h * 0.5f;
        float size = Mathf.Max(w, h) * scale;

        float x = Mathf.Clamp(cx - size * 0.5f, 0, 1 - size);
        float y = Mathf.Clamp(cy - size * 0.5f, 0, 1 - size);
        return new Rect(x, y, size, size);
    }
    List<Candidate> PerformPalmDetection()
    {
        TextureConverter.ToTensor(webCam, palmInputTensor, nhwcTransform);
        palmWorker.Schedule(palmInputTensor);

        Tensor<float> rawBoxes = palmWorker.PeekOutput(0) as Tensor<float>;
        Tensor<float> rawScores = palmWorker.PeekOutput(1) as Tensor<float>;

        if (rawBoxes == null || rawScores == null) return new List<Candidate>();

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
    List<Candidate> NonMaxSuppression(List<Candidate> candidates, float iouThresh, int maxResults)
    {
        var sorted = candidates.OrderByDescending(c => c.score).ToList();
        var kept = new List<Candidate>();

        while (sorted.Count > 0 && kept.Count < maxResults)
        {
            Candidate best = sorted[0];
            kept.Add(best);
            sorted.RemoveAt(0);
            sorted.RemoveAll(c => IoU(c.box, best.box) > iouThresh);
        }

        return kept;
    }
    List<Candidate> ParsePalmBBoxes(float[] boxData, float[] scoreData)
    {
        List<Candidate> candidates = new List<Candidate>();

        for (int i = 0; i < scoreData.Length; i++)
        {
            float confidence = 1f / (1f + Mathf.Exp(-scoreData[i]));
            if (confidence < scoreThreshold) continue;

            Vector2[] landMarks = new Vector2[9];
            for (int k = 0; k < 9; k++)
            {
                float kx = boxData[i * 18 + k * 2 + 0] / 192 + anchors[i].x;
                float ky = boxData[i * 18 + k * 2 + 1] / 192 + anchors[i].y;
                landMarks[k] = new Vector2(kx, ky);
            }

            float cx = landMarks[0].x;
            float cy = landMarks[0].y;
            float w = landMarks[1].x - anchors[i].x;
            float h = landMarks[2].y - anchors[i].y;

            float xMin = Mathf.Clamp01(cx - w * 0.5f);
            float yMin = Mathf.Clamp01(cy - h * 0.5f);

            Vector2 dir = landMarks[4] - landMarks[2];
            float handAngle = NormalizeRadians((0.5f * Mathf.PI) - Mathf.Atan2(-dir.y, dir.x));

            candidates.Add(new Candidate { box = new Rect(xMin, yMin, w, h), score = confidence, landMarks = landMarks, angle = handAngle });
        }

        return NonMaxSuppression(candidates, iouThreshold, maxHands);
    }

    float NormalizeRadians(float angle)
    {
        return angle - (2f * Mathf.PI) * Mathf.Floor((angle + Mathf.PI) / (2f * Mathf.PI));
    }
    float IoU(Rect a, Rect b)
    {
        float x1 = Mathf.Max(a.xMin, b.xMin);
        float y1 = Mathf.Max(a.yMin, b.yMin);
        float x2 = Mathf.Min(a.xMax, b.xMax);
        float y2 = Mathf.Min(a.yMax, b.yMax);

        float interW = Mathf.Max(0, x2 - x1);
        float interH = Mathf.Max(0, y2 - y1);
        float interArea = interW * interH;

        float unionArea = a.width * a.height + b.width * b.height - interArea;
        if (unionArea <= 0) return 0;
        return interArea / unionArea;
    }

    void CropHandRegion(Texture sourceTex, Rect area, float angleRadians)
    {
        RenderTexture.active = croppedHandBuffer;
        GL.Clear(true, true, Color.clear);

        GL.PushMatrix();
        GL.LoadOrtho();

        blitMaterial.mainTexture = sourceTex;
        blitMaterial.SetPass(0);

        Vector2 center = new Vector2(area.x + area.width * 0.5f, 1f - (area.y + area.height * 0.5f));
        Vector2 halfExtents = new Vector2(area.width * 0.5f, area.height * 0.5f);

        Vector2[] localCorners = new Vector2[4]
        {
            new Vector2(-halfExtents.x, -halfExtents.y),
            new Vector2(halfExtents.x, -halfExtents.y),
            new Vector2(halfExtents.x, halfExtents.y),
            new Vector2(-halfExtents.x, halfExtents.y)
        };

        float sin = Mathf.Sin(-angleRadians);
        float cos = Mathf.Cos(-angleRadians);

        GL.Begin(GL.QUADS);
        for (int i = 0; i < 4; i++)
        {
            float rotX = localCorners[i].x * cos - localCorners[i].y * sin;
            float rotY = localCorners[i].x * sin + localCorners[i].y * cos;
            Vector2 uv = center + new Vector2(rotX, rotY);

            GL.TexCoord2(uv.x, uv.y);

            if (i == 0) GL.Vertex3(0, 0, 0);
            if (i == 1) GL.Vertex3(1, 0, 0);
            if (i == 2) GL.Vertex3(1, 1, 0);
            if (i == 3) GL.Vertex3(0, 1, 0);
        }
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = null;
    }
    void ProcessLandmarks(Tensor<float> tensorData, Rect appliedCrop, float angleRadians)
    {
        if (tensorData.count >= 63)
        {
            float[] data = tensorData.DownloadToArray();

            float sin = Mathf.Sin(-angleRadians);
            float cos = Mathf.Cos(-angleRadians);

            Vector2 center = new Vector2(appliedCrop.x + appliedCrop.width * 0.5f, 1f - (appliedCrop.y + appliedCrop.height * 0.5f));

            handLandmarks = new Vector2[21];
            for (int i = 0; i < 63; i += 3)
            {
                float cropX = data[i] / 224f;
                float cropY = data[i + 1] / 224f;
                float cropZ = data[i + 2] / 224f;

                float cx = cropX - 0.5f;
                float cy = 0.5f - cropY;

                float rotX = cx * cos - cy * sin;
                float rotY = cx * sin + cy * cos;
                float rotZ = -cropZ;

                float globalX = center.x + rotX * appliedCrop.width;
                float globalY = center.y + rotY * appliedCrop.height;

                int size = Mathf.Max(Screen.width, Screen.height);
                float offsetX = (Screen.width - size) / 2f;
                float offsetY = (Screen.height - size) / 2f;

                float screenX = (globalX * size) + offsetX;
                float screenY = (globalY * size) + offsetY;
                if (flipCamera) screenX = Screen.width - screenX;

                handLandmarks[i / 3] = Camera.main.ScreenToWorldPoint(new Vector3(screenX, screenY, 10f));
            }
        }
        else
        {
            handLandmarks = null;
        }
    }

    public Vector2 palmCenter()
    {
        if (handLandmarks == null) return new Vector2(0, 0);
        return Vector3.Lerp(handLandmarks[0], handLandmarks[9], 0.5f);
    }

    private void OnDestroy()
    {
        palmWorker?.Dispose();
        worker?.Dispose();
        palmInputTensor?.Dispose();
        inputTensor?.Dispose();

        if (webCam != null)
        {
            webCam.Stop();
            Destroy(webCam);
        }

        if (blitMaterial != null) Destroy(blitMaterial);

        if (croppedHandBuffer != null)
        {
            croppedHandBuffer.Release();
            Destroy(croppedHandBuffer);
        }
    }
}