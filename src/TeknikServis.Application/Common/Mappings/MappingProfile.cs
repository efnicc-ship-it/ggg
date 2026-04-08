using AutoMapper;

namespace TeknikServis.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Feature-specific mappings are added in each feature's own profile
        // or via IMapFrom<T> interface
        ApplyMappingsFromAssembly(typeof(MappingProfile).Assembly);
    }

    private void ApplyMappingsFromAssembly(System.Reflection.Assembly assembly)
    {
        var mapFromType = typeof(IMapFrom<>);
        var mappingMethodName = nameof(IMapFrom<object>.Mapping);

        bool HasInterface(Type t) => t.IsGenericType && t.GetGenericTypeDefinition() == mapFromType;

        var types = assembly.GetExportedTypes().Where(t => t.GetInterfaces().Any(HasInterface)).ToList();

        var argumentTypes = new Type[] { typeof(Profile) };

        foreach (var type in types)
        {
            var instance = Activator.CreateInstance(type);
            var methodInfo = type.GetMethod(mappingMethodName);

            if (methodInfo != null)
            {
                methodInfo.Invoke(instance, new object[] { this });
            }
            else
            {
                var interfaces = type.GetInterfaces().Where(HasInterface);
                foreach (var @interface in interfaces)
                {
                    var interfaceMethodInfo = @interface.GetMethod(mappingMethodName, argumentTypes);
                    interfaceMethodInfo?.Invoke(instance, new object[] { this });
                }
            }
        }
    }
}
