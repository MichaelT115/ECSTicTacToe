using Unity.Entities;

/// <summary>
/// The update group for evaluating the current state of the board.
/// </summary>
[DisableAutoCreation]
[UpdateInGroup(typeof(BoardEvaluationUpdateGroup))]
public class BoardEvaluationUpdateGroup : ComponentSystemGroup { }
