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

    public void clear(string notes)
    {
        if ((!(notes != "") || !(Notes != notes)) && Index >= 0)
        {
            int index = Index;
            Index = -1;
            if (Main.projectile[index] != null && ((Entity)Main.projectile[index]).active && Main.projectile[index].type == Type && Main.projectile[index].owner == Main.myPlayer)
            {
                Main.projectile[index].Kill();
            }
        }
    }
}

