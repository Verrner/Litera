namespace Next.Litera.Scripting
{
    public abstract class BaseMathRuntimeNode : RuntimeNode
    {
        [RuntimeDataSource("a"), DataTransportInput("a")] public float a;
        [RuntimeDataSource("b"), DataTransportInput("b")] public float b;
        [DataTransportOutput("result")] public float result;
    }
}