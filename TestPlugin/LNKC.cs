namespace TestPlugin;

public class LNKC
{
    public int ID { get; set; }

    public long KC { get; set; }

    public LNKC(int id)
    {
        ID = id;
        KC = 1L;
    }

    public LNKC(int id, long kc)
    {
        ID = id;
        KC = kc;
    }
}
