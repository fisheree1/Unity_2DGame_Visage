public interface IState
{
    /// <summary>
    /// Called when entering the state.
    /// </summary>
    void OnEnter();
    /// <summary>
    /// Called every frame while in the state.
    /// </summary>
    void OnUpdate();
    /// <summary>
    /// Called when exiting the state.
    /// </summary>
    void OnExit();
}