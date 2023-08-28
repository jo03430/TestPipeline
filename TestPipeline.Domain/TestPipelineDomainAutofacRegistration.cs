using System.Diagnostics.CodeAnalysis;
using Secura.Infrastructure.Autofac;
using Secura.Infrastructure.Repositories;
using Autofac;
using Module = Autofac.Module;
using System.Reflection;
using Secura.Infrastructure.Services;

namespace TestPipeline.Domain;

[ExcludeFromCodeCoverage]
public class TestPipelineDomainAutofacRegistration : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterModule<RepositoryModule>();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AssignableTo<IService>()
            .AsImplementedInterfaces();

        builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
            .AssignableTo<IRepository>()
            .AsImplementedInterfaces();
    }
}