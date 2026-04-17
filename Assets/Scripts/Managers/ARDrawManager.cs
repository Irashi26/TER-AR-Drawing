using System.Collections.Generic;
using DilmerGames.Core.Singletons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using System.IO;
using System.Text;
using System;
using UnityEngine.EventSystems;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;
    
    public bool isDrawingVisible = true;

    // --- NOUVEAU : Variables pour le compteur de fichiers ---
    private int downloadCounter = 1;
    private string lastPhaseStored = "";

    [SerializeField]
    private UnityEvent OnDraw = null;

    [SerializeField]
    private ARAnchorManager anchorManager = null;

    [SerializeField] 
    private Camera arCamera = null;

    private List<ARAnchor> anchors = new List<ARAnchor>();

    private Dictionary<int, ARLine> Lines = new Dictionary<int, ARLine>();

    private bool CanDraw { get; set; }

    void Update ()
    {
        #if !UNITY_EDITOR    
        DrawOnTouch();
        #else
        DrawOnMouse();
        #endif
    }

    public void AllowDraw(bool isAllow)
    {
        CanDraw = isAllow;
    }

    void DrawOnTouch()
    {
        if(!CanDraw) return;

        int tapCount = Input.touchCount > 1 && lineSettings.allowMultiTouch ? Input.touchCount : 1;

        for(int i = 0; i < tapCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            Vector3 touchPosition = arCamera.ScreenToWorldPoint(new Vector3(Input.GetTouch(i).position.x, Input.GetTouch(i).position.y, lineSettings.distanceFromCamera));
            
            if(touch.phase == TouchPhase.Began)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return;

                OnDraw?.Invoke();
                
                ARAnchor anchor = anchorManager.AddAnchor(new Pose(touchPosition, Quaternion.identity));
                if (anchor != null) anchors.Add(anchor);

                ARLine line = new ARLine(lineSettings);
                Lines.Add(touch.fingerId, line);
                line.AddNewLineRenderer(transform, anchor, touchPosition);
                
                ApplyCurrentVisibility();
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (Lines.ContainsKey(touch.fingerId)) Lines[touch.fingerId].AddPoint(touchPosition);
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if (Lines.ContainsKey(touch.fingerId)) Lines.Remove(touch.fingerId);
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButton(0))
        {
            if(Lines.Keys.Count == 0)
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                
                OnDraw?.Invoke();
                ARLine line = new ARLine(lineSettings);
                Lines.Add(0, line);
                line.AddNewLineRenderer(transform, null, mousePosition);
                ApplyCurrentVisibility();
            }
            else 
            {
                Lines[0].AddPoint(mousePosition);
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            Lines.Remove(0);   
        }
    }

    GameObject[] GetAllLinesInScene()
    {
        return GameObject.FindGameObjectsWithTag("Line");
    }

    public void ClearLines()
    {
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines) { Destroy(currentLine); }
        Lines.Clear(); 
    }

    public void SetVisibility(bool visible)
    {
        isDrawingVisible = visible;
        ApplyCurrentVisibility();
    }

    public void RevealLines()
    {
        isDrawingVisible = true;
        ApplyCurrentVisibility();
    }

    private void ApplyCurrentVisibility()
    {
        GameObject[] lines = GetAllLinesInScene();
        foreach (GameObject currentLine in lines)
        {
            LineRenderer lr = currentLine.GetComponent<LineRenderer>();
            if (lr != null) lr.enabled = isDrawingVisible;
        }
    }

    // ==========================================
    // --- FONCTION DE TELECHARGEMENT AMÉLIORÉE ---
    // ==========================================
    public void DownloadDrawingCSV()
    {
        ExperimentManager expManager = FindObjectOfType<ExperimentManager>();
        if (expManager == null) return;

        // LOGIQUE DE COMPTEUR :
        // Si la phase a changé depuis le dernier téléchargement, on remet le compteur à 1
        if (expManager.currentPhase != lastPhaseStored)
        {
            downloadCounter = 1;
            lastPhaseStored = expManager.currentPhase;
        }

        StringBuilder csvText = new StringBuilder();
        csvText.AppendLine("Numero_Ligne,Index_Point,X,Y,Z"); 

        GameObject[] allLines = GetAllLinesInScene();
        int lineId = 0;
        
        foreach (GameObject currentLineObj in allLines)
        {
            LineRenderer lineRenderer = currentLineObj.GetComponent<LineRenderer>();
            if (lineRenderer == null) continue; 

            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            for (int i = 0; i < points.Length; i++)
            {
                // Note : Utilisation du point-virgule ou virgule selon ton Excel
                csvText.AppendLine($"{lineId};{i};{points[i].x};{points[i].y};{points[i].z}");
            }
            lineId++;
        }

        // Création du nom de fichier avec le suffixe du compteur
        string fileName = $"{expManager.participantID}_{expManager.groupType}_{expManager.currentPhase}_{downloadCounter}.csv";
        
        // On augmente le compteur pour le PROCHAIN clic dans cette phase
        downloadCounter++;

        string folderPath;
        #if UNITY_ANDROID && !UNITY_EDITOR
            folderPath = "/storage/emulated/0/Download";
        #else
            folderPath = Application.persistentDataPath;
        #endif

        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllText(filePath, csvText.ToString());
        
        ARDebugManager.Instance.LogInfo($"SAUVEGARDE : {fileName}");
    }
}