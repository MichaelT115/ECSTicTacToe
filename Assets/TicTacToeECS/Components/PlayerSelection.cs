using Unity.Entities;

/// <summary>
/// The index of the cell 
/// </summary>
struct PlayerSelection : IComponentData
{
    public static implicit operator PlayerSelection(int selectionIndex) 
        => new PlayerSelection() { selctionIndex = selectionIndex };
    public static implicit operator int(PlayerSelection playerSelection)
        => playerSelection.selctionIndex;

    public int selctionIndex;
}
