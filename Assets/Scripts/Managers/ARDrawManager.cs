using System.Collections.Generic;
using DilmerGames.Core.Singletons;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;
using System.IO;
using System.Text;
using System;

[RequireComponent(typeof(ARAnchorManager))]
public class ARDrawManager : Singleton<ARDrawManager>
{
    [SerializeField]
    private LineSettings lineSettings = null;

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
            
            ARDebugManager.Instance.LogInfo($"{touch.fingerId}");

            if(touch.phase == TouchPhase.Began)
            {
                OnDraw?.Invoke();
                
                ARAnchor anchor = anchorManager.AddAnchor(new Pose(touchPosition, Quaternion.identity));
                if (anchor == null) 
                    Debug.LogError("Error creating reference point");
                else 
                {
                    anchors.Add(anchor);
                    ARDebugManager.Instance.LogInfo($"Anchor created & total of {anchors.Count} anchor(s)");
                }

                ARLine line = new ARLine(lineSettings);
                Lines.Add(touch.fingerId, line);
                line.AddNewLineRenderer(transform, anchor, touchPosition);
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                Lines[touch.fingerId].AddPoint(touchPosition);
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                Lines.Remove(touch.fingerId);
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButton(0))
        {
            OnDraw?.Invoke();

            if(Lines.Keys.Count == 0)
            {
                ARLine line = new ARLine(lineSettings);
                Lines.Add(0, line);
                line.AddNewLineRenderer(transform, null, mousePosition);
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
        foreach (GameObject currentLine in lines)
        {
            LineRenderer line = currentLine.GetComponent<LineRenderer>();
            Destroy(currentLine);
        }
    }

    // --- NOUVEAU CODE : LE BOUTON TÉLÉCHARGER ---
    public void DownloadDrawingCSV()
    {
        // 1. On prépare une page blanche virtuelle
        StringBuilder csvText = new StringBuilder();
        
        // 2. On écrit l'en-tête du tableau (les colonnes Excel)
        csvText.AppendLine("Numero_Ligne,Index_Point,X,Y,Z"); 

        // 3. On fouille dans tous les traits dessinés
        int lineId = 0;
        foreach (var lineKvp in Lines)
        {
            ARLine currentLine = lineKvp.Value;
            Vector3[] points = currentLine.GetPositions();

            // 4. On écrit les coordonnées de chaque point un par un
            for (int i = 0; i < points.Length; i++)
            {
                csvText.AppendLine($"{lineId},{i},{points[i].x},{points[i].y},{points[i].z}");
            }
            lineId++;
        }

        // 5. On invente un nom de fichier avec la date et l'heure (ex: Dessin_20260413_1530.csv)
        string timeStamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        // 6. On trouve le dossier secret du téléphone où on a le droit d'écrire
        string filePath = Path.Combine(Application.persistentDataPath, $"Dessin_{timeStamp}.csv");

        // 7. On sauvegarde !
        File.WriteAllText(filePath, csvText.ToString());
        
        // 8. On affiche un message de réussite sur l'écran !
        ARDebugManager.Instance.LogInfo($"TELECHARGEMENT OK : {timeStamp}.csv");
        Debug.Log($"Fichier sauvegardé ici : {filePath}");
    }
}