namespace ApiBank.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ServiceActionAttribute : Attribute
{
    public string ServiceName { get; }

    public ServiceActionAttribute(string serviceName)
    {
        ServiceName = serviceName;
    }
}