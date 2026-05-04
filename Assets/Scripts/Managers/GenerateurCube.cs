using UnityEngine;

public class GenerateurCube : MonoBehaviour
{
    [Header("Dimensions du Cube")]
    public float taille = 0.5f;      
    public float epaisseur = 0.01f;  

    [Header("Apparence")]
    public Material materiauArete; // <-- C'EST ÇA QUI CHANGE TOUT

    void Start()
    {
        ConstruireCube();
    }

    public void ConstruireCube()
    {
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

        for (int i = 0; i < 12; i++)
        {
            CreerArete(sommets[aretes[i, 0]], sommets[aretes[i, 1]], i);
        }
    }

    void CreerArete(Vector3 start, Vector3 end, int index)
    {
        GameObject baton = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        baton.name = "Arete_" + index;
        baton.transform.SetParent(transform, false);

        baton.transform.localPosition = (start + end) / 2f;
        baton.transform.up = end - start;
        float dist = Vector3.Distance(start, end);
        baton.transform.localScale = new Vector3(epaisseur, dist / 2f, epaisseur);

        // On applique ton vrai matériau
        if (materiauArete != null) baton.GetComponent<Renderer>().material = materiauArete;

        Destroy(baton.GetComponent<Collider>());
    }
}