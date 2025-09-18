using Next.Litera.BuiltIn.RuntimeElements;
using System.Collections.Generic;

namespace Next.Litera
{
    public interface IRuntimeConnectionPort
    {
        List<RuntimeConnection> InputConnections { get; set; }
        List<RuntimeConnection> OutputConnections { get; set; }
    }
}