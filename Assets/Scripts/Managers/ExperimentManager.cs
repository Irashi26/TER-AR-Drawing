using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    [Header("Ecrans UI")]
    public GameObject ecran1_ID;
    public GameObject ecran2_Groupe;
    public GameObject ecran3_Experience;

    [Header("Elements 3D / AR")]
    public GameObject modele3D_Huit; // <-- NOUVEAU : La case pour ton 8 !

    [Header("Elements UI")]
    public InputField inputID;
    public Text textInfoExperience; 

    [Header("Donnees Experience")]
    public string participantID = "Inconnu";
    public string groupType = "Aucun"; 
    public string currentPhase = "Entrainement"; 

    void Start()
    {
        ecran1_ID.SetActive(true);
        ecran2_Groupe.SetActive(false);
        ecran3_Experience.SetActive(false);

        // <-- NOUVEAU : On éteint le 8 au démarrage !
        if (modele3D_Huit != null) modele3D_Huit.SetActive(false);
    }

    public void ValiderID()
    {
        if (inputID.text != "") // Si on a bien tapé quelque chose
        {
            participantID = inputID.text;
            ecran1_ID.SetActive(false);   
            ecran2_Groupe.SetActive(true); 
        }
        else
        {
            Debug.Log("Attention : La case est vide, je bloque !");
        }
    }

    public void ChoisirGroupeConcomitant()
    {
        groupType = "Co";
        LancerExperience();
    }

    public void ChoisirGroupeTerminal()
    {
        groupType = "Ter";
        LancerExperience();
    }

    private void LancerExperience()
    {
        ecran2_Groupe.SetActive(false);
        ecran3_Experience.SetActive(true);

        // <-- NOUVEAU : On allume le 8 quand l'expérience démarre !
        if (modele3D_Huit != null) modele3D_Huit.SetActive(true);

        if (textInfoExperience != null)
        {
            textInfoExperience.text = $"{participantID} - {groupType} - {currentPhase}";
        }
    }
}