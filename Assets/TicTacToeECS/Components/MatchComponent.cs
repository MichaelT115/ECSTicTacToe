using Unity.Entities;

struct MatchComponent : IComponentData
{


    public enum MatchType : byte {
        NOT_SET = 0,
        HORIZONTAL,
        VERTICAL,
        DIAGONAL
    }

    public Team team;
    public int startIndex;
    public int endIndex;
    public MatchType matchType;
}