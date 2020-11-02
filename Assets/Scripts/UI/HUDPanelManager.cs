using TMPro;
using UnityEngine;


public class HUDPanelManager : MonoBehaviour
{
    public GameObject MainMenu;
    public TextMeshProUGUI SelectedUnitText;
    public TextMeshProUGUI RoundText;
    public TextMeshProUGUI PhaseInfoText;

    public void ToggleMainMenu(bool newState) => MainMenu.SetActive(newState);

    public void NextRound(int round) => (RoundText.text, RoundText.enabled) = ($"Round {round}", true);

    public void StartRound() => RoundText.enabled = false;

    public void FightEnded(int winningTeam) => (RoundText.text, RoundText.enabled) = (FightEndedText(winningTeam), true);

    public void UpdatePhaseInfo(int actingTeam, int round, int roundPhase)
    {
        PhaseInfoText.color = TeamColor(actingTeam);
        PhaseInfoText.text = $"Round {round}. Phase {RoundPhase(roundPhase)}. Acting team {Team(actingTeam)}";
    }

    public void ShowSelectedUnit(Unit unit)
    {
        SelectedUnitText.text = $"{unit.Name} of the {Team(unit.team)} team";
        SelectedUnitText.color = TeamColor(unit.team);
        SelectedUnitText.enabled = true;
    }

    public void HideSelectedUnit() => SelectedUnitText.enabled = false;

    private string Team(int teamID) => teamID == 0 ? "Blue(0)" : "Red(1)";

    private string RoundPhase(int roundPhase) => roundPhase == 0 ? "Light(0)" : (roundPhase == 1 ? "Medium(1)" : "Heavy(2)");

    private string FightEndedText(int winningTeam) => winningTeam == -1 ? $"Noone has won." : $"Fight Ended. {Team(winningTeam)} team has won.";

    private Color TeamColor(int team) => team == 0 ? Color.blue : Color.red;
}
