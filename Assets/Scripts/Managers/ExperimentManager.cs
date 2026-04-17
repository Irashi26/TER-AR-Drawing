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

    [Header("Modèles 3D")]
    public GameObject modeleHuit; 
    public GameObject modeleCube; 

    [Header("Textes d'info")]
    public Text textEntrainement; 
    public Text textLearning;
    public Text textTest;

    [Header("Boutons Spéciaux")]
    public GameObject boutonRevealTerminal; 

    [Header("Données")]
    public InputField inputID;
    public string participantID = "Inconnu";
    public string groupType = "Aucun"; 
    public string currentPhase = "Entrainement"; 

    void Start()
    {
        ecran1_ID.SetActive(true);
        ecran2_Groupe.SetActive(false);
        ecran3_Entrainement.SetActive(false);
        ecran4_Learning.SetActive(false);
        ecran5_Test.SetActive(false);
        
        if (modeleHuit != null) modeleHuit.SetActive(false);
        if (modeleCube != null) modeleCube.SetActive(false);
        if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);

        if (ARDrawManager.Instance != null) ARDrawManager.Instance.AllowDraw(false);
    }

    public void ValiderID()
    {
        if (inputID.text != "") {
            participantID = inputID.text;
            ecran1_ID.SetActive(false);   
            ecran2_Groupe.SetActive(true); 
        }
    }

    public void ChoisirGroupeConcomitant() { groupType = "Co"; LancerEntrainement(); }
    public void ChoisirGroupeTerminal() { groupType = "Ter"; LancerEntrainement(); }

    private void LancerEntrainement()
    {
        currentPhase = "Entrainement";
        ecran2_Groupe.SetActive(false);
        ecran3_Entrainement.SetActive(true);
        if (modeleCube != null) modeleCube.SetActive(true);
        if (modeleHuit != null) modeleHuit.SetActive(false);
        
        if (ARDrawManager.Instance != null) {
            ARDrawManager.Instance.SetVisibility(true);
            ARDrawManager.Instance.AllowDraw(true);
        }
        ActualiserUI();
    }

    public void PasserALearning()
    {
        currentPhase = "Learning";
        ecran3_Entrainement.SetActive(false);
        ecran4_Learning.SetActive(true);
        if (modeleCube != null) modeleCube.SetActive(false);
        if (modeleHuit != null) modeleHuit.SetActive(true);
        
        AppliquerReglesVisibilite();
        ActualiserUI();
    }

    public void PasserATest()
    {
        currentPhase = "Test";
        ecran4_Learning.SetActive(false);
        ecran5_Test.SetActive(true);
        if (modeleHuit != null) modeleHuit.SetActive(true);
        if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
        
        if (ARDrawManager.Instance != null) ARDrawManager.Instance.SetVisibility(true);
        ActualiserUI();
    }

    // --- NOUVELLE FONCTION RESTART TRIAL ---
    public void RestartEssai()
    {
        if (ARDrawManager.Instance != null)
        {
            // 1. On efface les traits
            ARDrawManager.Instance.ClearLines();
            
            // 2. On ré-applique les règles de visibilité selon la phase et le groupe
            AppliquerReglesVisibilite();
        }
    }

    private void AppliquerReglesVisibilite()
    {
        if (currentPhase == "Learning" && groupType == "Ter") 
        {
            if (ARDrawManager.Instance != null) ARDrawManager.Instance.SetVisibility(false);
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(true);
        } 
        else 
        {
            if (ARDrawManager.Instance != null) ARDrawManager.Instance.SetVisibility(true);
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
        }
    }

    private void ActualiserUI()
    {
        string message = $"{participantID} - {groupType} - {currentPhase}";
        if (textEntrainement != null) textEntrainement.text = message;
        if (textLearning != null) textLearning.text = message;
        if (textTest != null) textTest.text = message;
    }
}