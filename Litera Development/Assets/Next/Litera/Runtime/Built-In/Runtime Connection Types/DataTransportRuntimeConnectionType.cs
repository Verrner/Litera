using Next.Litera.BuiltIn.RuntimeElements;
using Next.Litera.Scripting;
using System;
using System.Linq;
using System.Reflection;

namespace Next.Litera.BuiltIn.RuntimeConnectionTypes
{
    public abstract class DataTransportRuntimeConnectionType : RuntimeConnectionType
    {
        [RuntimeDataSource("output-member-key")] public string OutputKey;
        [RuntimeDataSource("input-member-key")] public string InputKey;

        public override void Execute(RuntimeConnection connection)
        {
            MemberInfo output = connection.Output.GetType().GetMembers().ToList().Find(f =>
            {
                DataTransportOutputAttribute a = f.GetCustomAttribute<DataTransportOutputAttribute>();
                return a != null && a.Key == OutputKey;
            });
            MemberInfo input = connection.Input.GetType().GetMembers().ToList().Find(f =>
            {
                DataTransportInputAttribute a = f.GetCustomAttribute<DataTransportInputAttribute>();
                return a != null && a.Key == InputKey;
            });
            if (output == null) throw new Exception($"Data transport output with key \"{OutputKey}\" is not found in {connection.Output.GetType()}");
            if (input == null) throw new Exception($"Data transport input with key \"{InputKey}\" is not found in {connection.Input.GetType()}");

            Transport(output, input, connection);
        }

        protected abstract void Transport(MemberInfo output, MemberInfo input, RuntimeConnection connection);
    }
}