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
    public GameObject ecran6_TestAveugle; // NOUVEAU

    [Header("Modèles 3D")]
    public GameObject modeleHuit; 
    public GameObject modeleCube; 
    public GameObject sphereDepart; // NOUVEAU

    [Header("Boutons Spéciaux")]
    public GameObject boutonRevealTerminal; 
    public GameObject boutonRevealTest;     
    public GameObject boutonRevealAveugle; // NOUVEAU

    // --- ON REMET LES TEXTES ICI ---
    [Header("Textes d'info")]
    public Text textEntrainement; 
    public Text textLearning;
    public Text textTest;
    public Text textAveugle; // NOUVEAU

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
        if (ecran6_TestAveugle != null) ecran6_TestAveugle.SetActive(false);
        
        if (modeleHuit != null) modeleHuit.SetActive(false);
        if (modeleCube != null) modeleCube.SetActive(false);
        if (sphereDepart != null) sphereDepart.SetActive(false);
        
        if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
        if (boutonRevealTest != null) boutonRevealTest.SetActive(false);
        if (boutonRevealAveugle != null) boutonRevealAveugle.SetActive(false);

        if (ARDrawManager.Instance != null) ARDrawManager.Instance.AllowDraw(false);
    }

    public void ValiderID() 
    { 
        if (inputID.text != "") { 
            participantID = inputID.text; 
            ecran1_ID.SetActive(false); 
            ecran2_Groupe.SetActive(true); 
            ActualiserUI(); // Mise à jour
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
        
        AppliquerReglesVisibilite();
        ActualiserUI(); // Mise à jour
    }

    public void PasserALearning()
    {
        currentPhase = "Learning";
        ecran3_Entrainement.SetActive(false);
        ecran4_Learning.SetActive(true);
        
        if (modeleCube != null) modeleCube.SetActive(false);
        if (modeleHuit != null) modeleHuit.SetActive(true);
        
        AppliquerReglesVisibilite();
        ActualiserUI(); // Mise à jour
    }

    public void PasserATest()
    {
        currentPhase = "Test";
        ecran4_Learning.SetActive(false);
        ecran5_Test.SetActive(true);
        
        if (modeleHuit != null) modeleHuit.SetActive(true);
        
        AppliquerReglesVisibilite();
        ActualiserUI(); // Mise à jour
    }

    // --- NOUVELLE FONCTION POUR LE TEST AVEUGLE ---
    public void PasserATestAveugle()
    {
        currentPhase = "TestAveugle";
        ecran5_Test.SetActive(false);
        if (ecran6_TestAveugle != null) ecran6_TestAveugle.SetActive(true);

        // On rallume le 8 et la sphère pour qu'il voie où commencer
        if (modeleHuit != null) modeleHuit.SetActive(true);
        if (sphereDepart != null) sphereDepart.SetActive(true);

        AppliquerReglesVisibilite();
        ActualiserUI();
    }

    public void RestartEssai() 
    { 
        if (ARDrawManager.Instance != null) { 
            ARDrawManager.Instance.ClearLines(); 
            AppliquerReglesVisibilite(); 
            
            // NOUVEAU : Si on recommence le test aveugle, on réaffiche le repère
            if (currentPhase == "TestAveugle")
            {
                if (modeleHuit != null) modeleHuit.SetActive(true);
                if (sphereDepart != null) sphereDepart.SetActive(true);
            }
        } 
    }

    private void AppliquerReglesVisibilite()
    {
        bool isTerminal = (groupType == "Ter");

        if (currentPhase == "Entrainement")
        {
            if (ARDrawManager.Instance != null) {
                ARDrawManager.Instance.AllowDraw(true);
                ARDrawManager.Instance.SetVisibility(true);
            }
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
            if (boutonRevealTest != null) boutonRevealTest.SetActive(false);
            if (boutonRevealAveugle != null) boutonRevealAveugle.SetActive(false);
        }
        else if (currentPhase == "Learning")
        {
            if (ARDrawManager.Instance != null) {
                ARDrawManager.Instance.SetVisibility(!isTerminal);
            }
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(isTerminal);
            if (boutonRevealTest != null) boutonRevealTest.SetActive(false);
            if (boutonRevealAveugle != null) boutonRevealAveugle.SetActive(false);
        }
        else if (currentPhase == "Test")
        {
            if (ARDrawManager.Instance != null) {
                ARDrawManager.Instance.SetVisibility(false);
            }
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
            if (boutonRevealTest != null) boutonRevealTest.SetActive(true);
            if (boutonRevealAveugle != null) boutonRevealAveugle.SetActive(false);
        }
        else if (currentPhase == "TestAveugle")
        {
            // Le trait est invisible pour tout le monde
            if (ARDrawManager.Instance != null) {
                ARDrawManager.Instance.SetVisibility(false);
            }
            if (boutonRevealTerminal != null) boutonRevealTerminal.SetActive(false);
            if (boutonRevealTest != null) boutonRevealTest.SetActive(false);
            if (boutonRevealAveugle != null) boutonRevealAveugle.SetActive(true);
        }
    }

    // --- MISE À JOUR DES TEXTES ---
    private void ActualiserUI()
    {
        string message = $"{participantID} - {groupType} - {currentPhase}";
        
        if (textEntrainement != null) textEntrainement.text = message;
        if (textLearning != null) textLearning.text = message;
        if (textTest != null) textTest.text = message;
        if (textAveugle != null) textAveugle.text = message;
    }
}