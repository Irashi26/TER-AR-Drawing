using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    [Header("Ecrans UI")]
    public GameObject ecran1_ID;
    public GameObject ecran2_Groupe;
    public GameObject ecran3_Experience;

    [Header("Elements 3D / AR")]
    public GameObject modele3D_Huit; 

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
        
        if (modele3D_Huit != null) modele3D_Huit.SetActive(false);

        // <-- NOUVEAU : On verrouille le pinceau au lancement de l'application !
        if (ARDrawManager.Instance != null)
        {
            ARDrawManager.Instance.AllowDraw(false);
        }
    }

    public void ValiderID()
    {
        if (inputID.text != "") 
        {
            participantID = inputID.text;
            ecran1_ID.SetActive(false);   
            ecran2_Groupe.SetActive(true); 
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
        
        if (modele3D_Huit != null) modele3D_Huit.SetActive(true);

        if (textInfoExperience != null)
        {
            textInfoExperience.text = $"{participantID} - {groupType} - {currentPhase}";
        }

        // <-- NOUVEAU : L'expérience commence, on déverrouille le pinceau !
        if (ARDrawManager.Instance != null)
        {
            ARDrawManager.Instance.AllowDraw(true);
        }
    }
}