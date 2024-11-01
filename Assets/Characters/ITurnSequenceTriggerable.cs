public interface ITurnSequenceTriggerable
{
    void OnStartPlayerTurn();
    void OnStartEnemyTurn();
    void OnEndstep();
    void OnEndTurn();
    void OnUpkeep();
}
