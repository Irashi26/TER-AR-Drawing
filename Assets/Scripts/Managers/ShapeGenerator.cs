using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class ShapeGenerator : MonoBehaviour
{
    [Header("Réglages de la forme")]
    public int resolution = 100;      // Nombre de points (plus c'est élevé, plus c'est lisse)
    public float width = 0.3f;        // Largeur (30 cm)
    public float height = 0.2f;       // Hauteur (20 cm)
    public float depth = 0.15f;       // Écart de profondeur au croisement (15 cm)
    
    [Header("Réglages visuels")]
    public float lineWidth = 0.01f;   // Épaisseur du trait (1 cm)
    public Material defaultMaterial;  // Le matériel pour donner de la couleur (facultatif)

    void Start()
    {
        GenerateFigure8();
    }

    void GenerateFigure8()
    {
        // 1. On récupère le pinceau (LineRenderer)
        LineRenderer line = GetComponent<LineRenderer>();
        line.positionCount = resolution + 1;
        line.useWorldSpace = false; // La forme suit l'objet auquel elle est attachée
        
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        if (defaultMaterial != null)
        {
            line.material = defaultMaterial;
        }

        // 2. On calcule les points mathématiques
        for (int i = 0; i <= resolution; i++)
        {
            // t représente le temps pour faire un tour complet (de 0 à 2*PI)
            float t = (float)i / resolution * 2.0f * Mathf.PI;

            // La formule magique du 8 en 3D
            float x = width * Mathf.Sin(t);
            float y = height * Mathf.Sin(2.0f * t);
            float z = depth * Mathf.Cos(t); // C'est ici qu'on crée l'écart de profondeur !

            // On pose le point
            line.SetPosition(i, new Vector3(x, y, z));
        }
    }
}