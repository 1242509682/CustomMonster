using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestPlugin;

public class 怪物指示物修改节
{
    public int 怪物ID = 0;

    public string 查标志 = "";

    public List<指示物组节> 指示物条件 = new List<指示物组节>();

    public int 范围内 = 0;

    public List<指示物节> 指示物修改 = new List<指示物节>();
}
