using Terraria;

namespace TestPlugin;

public class LPrj
{
    public int Index { get; set; }

    public int Type { get; set; }

    public int UseI { get; set; }

    public string Notes { get; set; }

    public LPrj(int index, int useIndex, int type, string notes)
    {
        Index = index;
        UseI = useIndex;
        Type = type;
        Notes = notes;
    }
}

