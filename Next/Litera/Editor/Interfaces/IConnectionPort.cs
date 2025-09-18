using Next.Litera.BuiltIn.OtherGraphElements;
using System.Collections.Generic;
using UnityEngine;

namespace Next.Litera
{
    public interface IConnectionPort
    {
        Vector2 GetInputPortPosition();
        Vector2 GetOutputPortPosition();
        void RegisterCallbacks(Connection connection);
        void UnregisterCallbacks(Connection connection);
        void RemoveFromOutputConnections(Connection connection);
        void RemoveFromInputConnections(Connection connection);
        void AddToOutputConnections(Connection connection);
        void AddToInputConnections(Connection connection);
        List<Connection> GetOutputConnections();
        List<Connection> GetInputConnections();
    }
}