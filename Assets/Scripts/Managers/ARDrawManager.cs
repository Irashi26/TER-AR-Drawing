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

    public float precisionTolerance = 0.02f; // 2 cm de tolérance

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

        if (expManager.currentPhase != lastPhaseStored)
        {
            downloadCounter = 1;
            lastPhaseStored = expManager.currentPhase;
        }

        int totalPoints = 0;
        int precisePoints = 0;

        StringBuilder csvText = new StringBuilder();
        csvText.AppendLine("Numero_Ligne;Index_Point;X;Y;Z;Distance_Erreur"); 

        GameObject[] allLines = GameObject.FindGameObjectsWithTag("Line");
        int lineId = 0;
        
        foreach (GameObject currentLineObj in allLines)
        {
            LineRenderer lineRenderer = currentLineObj.GetComponent<LineRenderer>();
            if (lineRenderer == null) continue; 

            Vector3[] points = new Vector3[lineRenderer.positionCount];
            lineRenderer.GetPositions(points);

            for (int i = 0; i < points.Length; i++)
            {
                // 1. On calcule la distance avec le modèle 3D
                float dist = GetDistanceToActiveShape(points[i]);
                
                // 2. On compte pour le score
                totalPoints++;
                if (dist <= precisionTolerance) precisePoints++;

                // 3. On écrit dans le fichier (avec la distance en bonus !)
                csvText.AppendLine($"{lineId};{i};{points[i].x};{points[i].y};{points[i].z};{dist}");
            }
            lineId++;
        }

        // Calcul du pourcentage final
        float score = (totalPoints > 0) ? ((float)precisePoints / totalPoints) * 100f : 0f;

        // On crée le contenu final avec le score tout en haut
        string finalCsvContent = $"SCORE_PRECISION_POURCENT;{score:F2}\n" + csvText.ToString();

        string fileName = $"{expManager.participantID}_{expManager.groupType}_{expManager.currentPhase}_{downloadCounter}.csv";
        downloadCounter++;

        string folderPath = Application.persistentDataPath;
        #if UNITY_EDITOR
            folderPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        #elif UNITY_ANDROID
            folderPath = "/storage/emulated/0/Download";
        #endif

        string filePath = Path.Combine(folderPath, fileName);
        File.WriteAllText(filePath, finalCsvContent);
        
        Debug.Log($"⭐⭐⭐ SAUVEGARDE RÉUSSIE ! Score : {score:F2}% | Fichier : {filePath} ⭐⭐⭐");
    }

    // Le "Radar" qui mesure la distance entre le doigt et le modèle 3D
    private float GetDistanceToActiveShape(Vector3 p) 
    {
        ExperimentManager exp = FindObjectOfType<ExperimentManager>();
        
        // On cible le modèle Huit en priorité (car c'est là qu'on évalue vraiment)
        GameObject target = (exp.modeleHuit != null && exp.modeleHuit.activeInHierarchy) ? exp.modeleHuit : null;
        if (target == null) return 999f; // Si pas de modèle, on met une erreur immense

        float minDist = float.MaxValue;
        
        // On cherche le LineRenderer du 8
        foreach (var lr in target.GetComponentsInChildren<LineRenderer>()) 
        {
            Vector3[] nodes = new Vector3[lr.positionCount];
            lr.GetPositions(nodes);
            foreach (var node in nodes) 
            {
                // TransformPoint convertit la position du 8 dans l'espace réel
                float d = Vector3.Distance(p, lr.transform.TransformPoint(node));
                if (d < minDist) minDist = d;
            }
        }
        return minDist;
    }
}