public interface ITurnSequenceTriggerable
{
    void OnStartPlayerTurn();
    void OnStartEnemyTurn();
    void OnEndTurn();
    void OnStartPlayerSimulationTurn();
    void OnStartEnemySimulationTurn();
    void OnEndSimulationTurn();
}
