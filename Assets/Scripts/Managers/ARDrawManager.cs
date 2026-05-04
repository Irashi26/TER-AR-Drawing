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
    [SerializeField] private LineSettings lineSettings = null;
    
    public bool isDrawingVisible = true;

    [Header("Feedback Visuel (Couleurs)")]
    public float distVerte = 0.02f; // De 0 à 2cm : Vert vers Jaune
    public float distRouge = 0.05f; // De 2cm à 5cm : Jaune vers Rouge

    [Header("Références AR")]
    [SerializeField] private UnityEvent OnDraw = null;
    [SerializeField] private ARAnchorManager anchorManager = null;
    [SerializeField] private Camera arCamera = null;

    // --- Variables pour le CSV et le Temps ---
    private int downloadCounter = 1;
    private string lastPhaseStored = "";
    private float currentLineStartTime;
    private List<float> lineDurations = new List<float>();

    // --- Variables pour la téléportation ---
    private Vector3 positionInitialeHuit;
    private bool positionHuitSauvegardee = false;

    // --- Système de mini-segments ---
    private Dictionary<int, GameObject> activeTraces = new Dictionary<int, GameObject>();
    private Dictionary<int, Vector3> lastPointPositions = new Dictionary<int, Vector3>();
    private bool CanDraw { get; set; }

    void Update()
    {
        #if !UNITY_EDITOR    
        DrawOnTouch();
        #else
        DrawOnMouse();
        #endif
    }

    public void AllowDraw(bool isAllow) { CanDraw = isAllow; }

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
                StartNewTrace(touch.fingerId, touchPosition);
            }
            else if(touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
            {
                if (activeTraces.ContainsKey(touch.fingerId))
                {
                    if (Vector3.Distance(lastPointPositions[touch.fingerId], touchPosition) > lineSettings.minDistanceBeforeNewPoint)
                    {
                        AddSegmentToTrace(touch.fingerId, touchPosition);
                    }
                }
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                if (activeTraces.ContainsKey(touch.fingerId)) EndTrace(touch.fingerId);
            }
        }
    }

    void DrawOnMouse()
    {
        if(!CanDraw) return;

        Vector3 mousePosition = arCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, lineSettings.distanceFromCamera));

        if(Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current.IsPointerOverGameObject()) return;
            StartNewTrace(0, mousePosition);
        }
        else if(Input.GetMouseButton(0))
        {
            if (activeTraces.ContainsKey(0))
            {
                if (Vector3.Distance(lastPointPositions[0], mousePosition) > lineSettings.minDistanceBeforeNewPoint)
                {
                    AddSegmentToTrace(0, mousePosition);
                }
            }
        }
        else if(Input.GetMouseButtonUp(0))
        {
            if (activeTraces.ContainsKey(0)) EndTrace(0);
        }
    }

    // ==========================================
    // --- LOGIQUE DES MINI-SEGMENTS COLORÉS ---
    // ==========================================

    private void StartNewTrace(int pointerId, Vector3 startPos)
    {
        OnDraw?.Invoke();
        currentLineStartTime = Time.time;

        // --- MAGIE DU TEST AVEUGLE (Téléportation et Disparition) ---
        ExperimentManager exp = FindObjectOfType<ExperimentManager>();
        if (exp != null && exp.currentPhase == "TestAveugle")
        {
            if (exp.modeleHuit != null && exp.sphereDepart != null)
            {
                // Sauvegarde de la position du 8 la première fois
                if (!positionHuitSauvegardee)
                {
                    positionInitialeHuit = exp.modeleHuit.transform.position;
                    positionHuitSauvegardee = true;
                }

                // 1. Calcul du décalage exact entre le doigt et la bille rouge
                Vector3 decalage = startPos - exp.sphereDepart.transform.position;
                
                // 2. Téléportation discrète de tout le 8
                exp.modeleHuit.transform.position += decalage;
                
                // 3. Disparition visuelle (le LineRenderer s'éteint, mais l'objet reste pour calculer l'erreur)
                LineRenderer lrHuit = exp.modeleHuit.GetComponent<LineRenderer>();
                if (lrHuit != null) lrHuit.enabled = false;
                
                // 4. Disparition de la bille
                exp.sphereDepart.SetActive(false); 
            }
        }
        // -------------------------------------------------------------

        // Création du parent qui va contenir tous les segments
        GameObject traceParent = new GameObject("Trace_" + DateTime.Now.Ticks);
        traceParent.tag = "Line";
        traceParent.transform.SetParent(transform);

        // --- CORRECTION : SÉCURITÉ POUR LE TEST SUR PC ---
        if (anchorManager != null && anchorManager.subsystem != null && anchorManager.subsystem.running)
        {
            ARAnchor anchor = anchorManager.AddAnchor(new Pose(startPos, Quaternion.identity));
            if (anchor != null) traceParent.transform.SetParent(anchor.transform);
        }

        activeTraces.Add(pointerId, traceParent);
        lastPointPositions.Add(pointerId, startPos);
    }

    private void AddSegmentToTrace(int pointerId, Vector3 endPos)
    {
        Vector3 startPos = lastPointPositions[pointerId];
        GameObject traceParent = activeTraces[pointerId];

        // 1. Calcul de la distance au modèle
        float distance = GetDistanceToActiveShape(endPos);
        
        // 2. Détermination du dégradé de couleur
        Color segmentColor = GetColorFromDistance(distance);

        // 3. Création visuelle du segment
        GameObject segObj = new GameObject("Segment");
        segObj.transform.SetParent(traceParent.transform);
        
        LineRenderer lr = segObj.AddComponent<LineRenderer>();
        // Matériel spécial pour les couleurs brutes qui "pètent" bien
        lr.material = new Material(Shader.Find("Sprites/Default")); 
        lr.startColor = lr.endColor = segmentColor;
        lr.startWidth = lr.endWidth = lineSettings.startWidth; // Épaisseur fine
        lr.positionCount = 2;
        lr.SetPosition(0, startPos);
        lr.SetPosition(1, endPos);
        lr.useWorldSpace = true;
        
        // 4. On applique la règle d'invisibilité (Le terminal ne verra rien jusqu'au bouton "Voir")
        lr.enabled = isDrawingVisible;

        lastPointPositions[pointerId] = endPos;
    }

    private void EndTrace(int pointerId)
    {
        activeTraces.Remove(pointerId);
        lastPointPositions.Remove(pointerId);
        lineDurations.Add(Time.time - currentLineStartTime);
    }

    private Color GetColorFromDistance(float d)
    {
        // De 0 à 2 cm : Transition Vert -> Jaune
        if (d <= distVerte) return Color.Lerp(Color.green, Color.yellow, d / distVerte);
        // De 2 cm à 5 cm : Transition Jaune -> Rouge
        if (d <= distRouge) return Color.Lerp(Color.yellow, Color.red, (d - distVerte) / (distRouge - distVerte));
        // Plus de 5 cm : Rouge
        return Color.red;
    }

    // ==========================================
    // --- GESTION DE LA VISIBILITÉ ---
    // ==========================================

    public void SetVisibility(bool visible)
    {
        isDrawingVisible = visible;
        ApplyCurrentVisibility();
    }

    public void RevealLines()
    {
        isDrawingVisible = true;
        ApplyCurrentVisibility();

        // NOUVEAU : Réafficher le 8 si on était en test aveugle
        ExperimentManager exp = FindObjectOfType<ExperimentManager>();
        if (exp != null && exp.modeleHuit != null)
        {
            LineRenderer lrHuit = exp.modeleHuit.GetComponent<LineRenderer>();
            if (lrHuit != null) lrHuit.enabled = true; // On rallume le 8 pour la comparaison
        }
    }

    private void ApplyCurrentVisibility()
    {
        foreach (GameObject trace in GameObject.FindGameObjectsWithTag("Line"))
        {
            foreach (LineRenderer lr in trace.GetComponentsInChildren<LineRenderer>())
            {
                lr.enabled = isDrawingVisible;
            }
        }
    }

    public void ClearLines()
    {
        foreach (GameObject trace in GameObject.FindGameObjectsWithTag("Line")) Destroy(trace);
        activeTraces.Clear();
        lastPointPositions.Clear();
        lineDurations.Clear(); 

        // NOUVEAU : Gérer le reset du test aveugle
        ExperimentManager exp = FindObjectOfType<ExperimentManager>();
        if (exp != null && exp.modeleHuit != null)
        {
            LineRenderer lrHuit = exp.modeleHuit.GetComponent<LineRenderer>();
            if (lrHuit != null) lrHuit.enabled = true; // On s'assure que le 8 est visible
            
            if (exp.currentPhase == "TestAveugle") 
            {
                if (exp.sphereDepart != null) exp.sphereDepart.SetActive(true);
                
                // On remet le 8 à sa position d'origine pour ne pas fausser le prochain essai
                if (positionHuitSauvegardee)
                {
                    exp.modeleHuit.transform.position = positionInitialeHuit;
                }
            }
        }
    }

    // ==========================================
    // --- L'EXPORT CSV PARFAIT ---
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
        int greenPoints = 0;  // Pour le score strict
        int yellowPoints = 0; // Pour le score partiel

        StringBuilder csvText = new StringBuilder();
        csvText.AppendLine("Numero_Ligne;Duree_Trait_Sec;Index_Point;X;Y;Z;Distance_Erreur"); 

        GameObject[] allTraces = GameObject.FindGameObjectsWithTag("Line");
        System.Array.Sort(allTraces, (a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));

        for (int lineId = 0; lineId < allTraces.Length; lineId++)
        {
            float duration = (lineId < lineDurations.Count) ? lineDurations[lineId] : 0f;
            LineRenderer[] segments = allTraces[lineId].GetComponentsInChildren<LineRenderer>();
            if (segments.Length == 0) continue;

            List<Vector3> points = new List<Vector3>();
            points.Add(segments[0].GetPosition(0)); 
            for (int s = 0; s < segments.Length; s++) points.Add(segments[s].GetPosition(1));

            for (int i = 0; i < points.Count; i++)
            {
                float dist = GetDistanceToActiveShape(points[i]);
                totalPoints++;

                // LOGIQUE DES SCORES
                if (dist <= distVerte) 
                {
                    greenPoints++; // Zone Verte
                }
                else if (dist <= distRouge) 
                {
                    yellowPoints++; // Zone Jaune
                }

                csvText.AppendLine($"{lineId};{duration:F3};{i};{points[i].x};{points[i].y};{points[i].z};{dist:F4}");
            }
        }

        // --- CALCULS DES DEUX SCORES ---
        float scoreStrict = (totalPoints > 0) ? ((float)greenPoints / totalPoints) * 100f : 0f;
        float pointsPonderes = (float)greenPoints + (yellowPoints * 0.5f);
        float scorePartiel = (totalPoints > 0) ? (pointsPonderes / totalPoints) * 100f : 0f;

        // --- GÉNÉRATION DU CONTENU DU FICHIER ---
        string finalCsvContent = $"SCORE_PRECISION_STRICT;{scoreStrict:F2}\n" + 
                                 $"SCORE_PERFORMANCE_PARTIEL;{scorePartiel:F2}\n" + 
                                 csvText.ToString();

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
        
        Debug.Log($"⭐⭐⭐ SCORES : Strict={scoreStrict:F2}% | Partiel={scorePartiel:F2}% ⭐⭐⭐");
    }

    private float GetDistanceToActiveShape(Vector3 p) 
    {
        ExperimentManager exp = FindObjectOfType<ExperimentManager>();
        
        GameObject target = (exp.modeleHuit != null && exp.modeleHuit.activeInHierarchy) ? exp.modeleHuit : null;
        if (target == null) return 999f; 

        float minDist = float.MaxValue;
        
        foreach (var lr in target.GetComponentsInChildren<LineRenderer>()) 
        {
            Vector3[] nodes = new Vector3[lr.positionCount];
            lr.GetPositions(nodes);
            
            // On vérifie la distance par rapport à chaque SEGMENT de la ligne (Haute Précision)
            for (int i = 0; i < nodes.Length - 1; i++) 
            {
                Vector3 a = lr.transform.TransformPoint(nodes[i]);
                Vector3 b = lr.transform.TransformPoint(nodes[i+1]);
                
                float d = DistancePointToSegment(p, a, b);
                if (d < minDist) minDist = d;
            }
        }
        return minDist;
    }

    // Fonction mathématique pour calculer la distance entre le doigt et un bout de ligne
    private float DistancePointToSegment(Vector3 point, Vector3 start, Vector3 end)
    {
        Vector3 lineDirection = end - start;
        Vector3 pointToStart = point - start;
        
        float t = Vector3.Dot(pointToStart, lineDirection) / lineDirection.sqrMagnitude;
        t = Mathf.Clamp01(t); // Force le point à rester sur le segment
        
        Vector3 closestPoint = start + t * lineDirection;
        return Vector3.Distance(point, closestPoint);
    }
}