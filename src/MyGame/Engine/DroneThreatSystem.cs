namespace MyGame.Engine;

/// <summary>
/// Encapsulates drone threat logic: tracking threat level, emitting warnings,
/// and triggering the lose condition when the threshold is reached.
///
/// DroneThreatLevel and DroneThreatThreshold remain on GameState for save/load
/// compatibility — this class reads and writes them through the state reference.
/// </summary>
public class DroneThreatSystem
{
    private readonly GameState _state;

    public DroneThreatSystem(GameState state) => _state = state;

    /// <summary>Returns true if the player's current room is a high-risk drone zone.</summary>
    public bool IsHighRiskRoom() =>
        _state.HighRiskRoomIds.Contains(_state.CurrentRoomId);

    /// <summary>
    /// Increments the threat level and returns the appropriate warning message,
    /// or null if the threshold was reached (game over is set on GameState) or
    /// no message applies.
    /// </summary>
    public string? Increment()
    {
        _state.DroneThreatLevel++;

        if (_state.DroneThreatLevel >= _state.DroneThreatThreshold)
        {
            _state.HasLost = true;
            _state.IsRunning = false;
            return null;
        }

        return _state.DroneThreatLevel switch
        {
            1 => GameMessages.Drone.Warning1,
            2 => GameMessages.Drone.Warning2,
            3 => GameMessages.Drone.Warning3,
            _ => null
        };
    }

    /// <summary>Resets the threat level to zero (used on game restart).</summary>
    public void Reset() => _state.DroneThreatLevel = 0;
}
