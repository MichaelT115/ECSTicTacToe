using Unity.Collections;
using Unity.Entities;
// Checks if the player has reached an end turn condition.
// A turn is over if a validated selection was made.
public class EndTurnConditionSystem : ComponentSystem
{
    EntityQuery playerQuery;

    protected override void OnCreate()
    {
        playerQuery = GetEntityQuery(typeof(PlayerTeamComponent), typeof(HasTurnComponent), typeof(MadeSelectionComponent));
    }

    protected override void OnUpdate()
    {
        foreach (Entity entity in playerQuery.ToEntityArray(Allocator.Temp))
        {
           
        }
    }
}