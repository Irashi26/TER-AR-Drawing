using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    [Header("Ecrans UI")]
    public GameObject ecran1_ID;
    public GameObject ecran2_Groupe;
    public GameObject ecran3_Entrainement;
    public GameObject ecran4_Learning;
    public GameObject ecran5_Test;

    [Header("Elements 3D / AR")]
    public GameObject modele3D_Huit; 

    [Header("Elements UI")]
    public InputField inputID;
    
    // NOUVEAU : On a remplacé l'ancien texte unique par 3 cases !
    [Header("Textes d'info (Glisse les Text de chaque écran ici)")]
    public Text textEntrainement; 
    public Text textLearning;
    public Text textTest;

    [Header("Donnees Experience")]
    public string participantID = "Inconnu";
    public string groupType = "Aucun"; 
    public string currentPhase = "Entrainement"; 

    void Start()
    {
        // On éteint tout sauf l'écran 1
        ecran1_ID.SetActive(true);
        ecran2_Groupe.SetActive(false);
        ecran3_Entrainement.SetActive(false);
        ecran4_Learning.SetActive(false);
        ecran5_Test.SetActive(false);
        
        if (modele3D_Huit != null) modele3D_Huit.SetActive(false);

        if (ARDrawManager.Instance != null) ARDrawManager.Instance.AllowDraw(false);
    }

    // 1. Ecran ID -> Ecran Groupe
    public void ValiderID()
    {
        if (inputID.text != "") 
        {
            participantID = inputID.text;
            ecran1_ID.SetActive(false);   
            ecran2_Groupe.SetActive(true); 
        }
    }

    // 2. Ecran Groupe -> Ecran Entrainement
    public void ChoisirGroupeConcomitant() { groupType = "Co"; LancerEntrainement(); }
    public void ChoisirGroupeTerminal() { groupType = "Ter"; LancerEntrainement(); }

    private void LancerEntrainement()
    {
        currentPhase = "Entrainement";
        ecran2_Groupe.SetActive(false);
        ecran3_Entrainement.SetActive(true);
        DemarrerPhaseCommune();
    }

    // 3. Ecran Entrainement -> Ecran Learning
    public void PasserALearning()
    {
        currentPhase = "Learning";
        ecran3_Entrainement.SetActive(false);
        ecran4_Learning.SetActive(true);
        ActualiserUI();
    }

    // 4. Ecran Learning -> Ecran Test
    public void PasserATest()
    {
        currentPhase = "Test";
        ecran4_Learning.SetActive(false);
        ecran5_Test.SetActive(true);
        ActualiserUI();
    }

    // Fonctions utilitaires
    private void DemarrerPhaseCommune()
    {
        if (modele3D_Huit != null) modele3D_Huit.SetActive(true);
        if (ARDrawManager.Instance != null) ARDrawManager.Instance.AllowDraw(true);
        ActualiserUI();
    }

    // NOUVEAU : La fonction qui met à jour les 3 écrans d'un coup
    private void ActualiserUI()
    {
        string message = $"{participantID} - {groupType} - {currentPhase}";

        if (textEntrainement != null) textEntrainement.text = message;
        if (textLearning != null) textLearning.text = message;
        if (textTest != null) textTest.text = message;
    }
}