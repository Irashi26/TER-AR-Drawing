using UnityEngine;

public class GenerateurCube : MonoBehaviour
{
    [Header("Dimensions du Cube")]
    public float taille = 0.5f;      // Augmenté à 50cm pour mieux voir sur PC
    public float epaisseur = 0.01f;  // 1cm d'épaisseur

    [Header("Apparence")]
    public Color couleurArete = Color.cyan; // Une belle couleur turquoise/bleue

    void Start()
    {
        ConstruireCube();
    }

    public void ConstruireCube()
    {
        // On nettoie si on l'appelle plusieurs fois
        foreach (Transform child in transform) Destroy(child.gameObject);

        float d = taille / 2f;
        Vector3[] sommets = {
            new Vector3(-d, -d, -d), new Vector3(d, -d, -d),
            new Vector3(d, d, -d), new Vector3(-d, d, -d),
            new Vector3(-d, -d, d), new Vector3(d, -d, d),
            new Vector3(d, d, d), new Vector3(-d, d, d)
        };

        int[,] aretes = {
            {0,1}, {1,2}, {2,3}, {3,0}, {4,5}, {5,6}, {6,7}, {7,4}, {0,4}, {1,5}, {2,6}, {3,7}
        };

        // Création d'un matériau simple pour la couleur
        Material materialCouleur = new Material(Shader.Find("Unlit/Color"));
        materialCouleur.color = couleurArete;

        for (int i = 0; i < 12; i++)
        {
            CreerArete(sommets[aretes[i, 0]], sommets[aretes[i, 1]], i, materialCouleur);
        }
    }

    void CreerArete(Vector3 start, Vector3 end, int index, Material mat)
    {
        GameObject baton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baton.name = "Arete_" + index;
        baton.transform.SetParent(transform, false);

        // Position et Rotation
        baton.transform.localPosition = (start + end) / 2f;
        baton.transform.up = end - start;

        // Mise à l'échelle (Le cylindre Unity fait 2m de haut par défaut, d'où le /2)
        float dist = Vector3.Distance(start, end);
        baton.transform.localScale = new Vector3(epaisseur, dist / 2f, epaisseur);

        // Application du matériau
        baton.GetComponent<Renderer>().material = mat;

        // Supprimer le collider pour ne pas gêner le dessin
        Destroy(baton.GetComponent<Collider>());
    }
}