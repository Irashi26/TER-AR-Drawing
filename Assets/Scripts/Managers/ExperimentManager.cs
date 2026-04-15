using UnityEngine;
using UnityEngine.UI;

public class ExperimentManager : MonoBehaviour
{
    [Header("Ecrans UI")]
    public GameObject ecran1_ID;
    public GameObject ecran2_Groupe;
    public GameObject ecran3_Experience;

    [Header("Elements UI")]
    public InputField inputID;
    public Text textInfoExperience; // Le texte en bas à gauche

    [Header("Donnees Experience")]
    public string participantID = "Inconnu";
    public string groupType = "Aucun"; 
    public string currentPhase = "Entrainement"; // La première phase par défaut

    void Start()
    {
        // Au lancement, on s'assure que seul l'écran 1 est allumé
        ecran1_ID.SetActive(true);
        ecran2_Groupe.SetActive(false);
        ecran3_Experience.SetActive(false);
    }

    // --- FONCTIONS POUR LES BOUTONS ---

    // 1. Appelée par le bouton "SUIVANT" de l'écran 1
    public void ValiderID()
    {
        if (inputID.text != "") // Si la case n'est pas vide
        {
            participantID = inputID.text; // On sauvegarde le texte tapé
            ecran1_ID.SetActive(false);   // On éteint l'écran 1
            ecran2_Groupe.SetActive(true); // On allume l'écran 2
        }
    }

    // 2. Appelée par le bouton "CONCOMITANT" de l'écran 2
    public void ChoisirGroupeConcomitant()
    {
        groupType = "Co";
        LancerExperience();
    }

    // 3. Appelée par le bouton "TERMINAL" de l'écran 2
    public void ChoisirGroupeTerminal()
    {
        groupType = "Ter";
        LancerExperience();
    }

    // Fonction interne pour passer à l'écran de dessin
    private void LancerExperience()
    {
        ecran2_Groupe.SetActive(false);
        ecran3_Experience.SetActive(true);

        // On met à jour le petit texte en bas à gauche !
        if (textInfoExperience != null)
        {
            textInfoExperience.text = $"{participantID} - {groupType} - {currentPhase}";
        }
    }
}