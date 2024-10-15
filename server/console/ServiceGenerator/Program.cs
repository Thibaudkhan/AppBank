using System;
using System.IO;

namespace ServiceGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: service-generator <ServiceName>");
                return;
            }

            var serviceName = args[0];
            CreateService(serviceName);
            Console.WriteLine($"Service {serviceName} created successfully.");
        }

        static void CreateService(string serviceName)
        {
            string projectRoot = @"H:\code\ApiBank";
            string servicesFolder = Path.Combine(projectRoot, "src", "ApiBank.Infrastructure", "ApiServices");
            string applicationFolder = Path.Combine(projectRoot, "src", "ApiBank.Application", "UseCases", "Commands", serviceName);
            string enumFilePath = Path.Combine(projectRoot, "src", "ApiBank.Core", "Enums", "ServiceType.cs");
            string programFilePath = Path.Combine(projectRoot, "src", "ApiBank.Api", "Program.cs");
            string diFilePath = Path.Combine(projectRoot, "src", "ApiBank.Api", "ServiceDependencyInjection.cs");
            string serviceClassContent = $@"
using ApiBank.Core;
using System.Collections.Generic;
using ApiBank.Core.Abstracts;

namespace ApiBank.Infrastructure.ApiServices
{{
    public class {serviceName} : ApiService
    {{
        public override string Name => ""{serviceName}"";


        public override List<ActionCommand> GetAvailableCommands() => new List<ActionCommand>();
    }}
}}
";
            string serviceFilePath = Path.Combine(servicesFolder, $"{serviceName}.cs");
            Directory.CreateDirectory(servicesFolder);
            File.WriteAllText(serviceFilePath, serviceClassContent, System.Text.Encoding.UTF8);

            Directory.CreateDirectory(applicationFolder);

            if (File.Exists(diFilePath))
            {
                string diContent = File.ReadAllText(diFilePath);
                string diRegistration = $"          services.AddScoped<ApiBank.Infrastructure.ApiServices.{serviceName}>();";
                int insertIndex = diContent.IndexOf("public static void AddApiBankServices(this IServiceCollection services)");
                if (insertIndex != -1)
                {
                    int methodBodyIndex = diContent.IndexOf("{", insertIndex) + 1;
                    diContent = diContent.Insert(methodBodyIndex, Environment.NewLine + diRegistration);
                    File.WriteAllText(diFilePath, diContent, System.Text.Encoding.UTF8);
                }
            }
            else
            {
                throw new FileNotFoundException("ServiceDependencyInjection file not found", diFilePath);
            }
            
            if (File.Exists(enumFilePath))
            {
                string enumContent = File.ReadAllText(enumFilePath);
                int insertIndex = enumContent.LastIndexOf('}');
                if (insertIndex != -1)
                {
                    enumContent = enumContent.Insert(insertIndex - 1, $"    public const string {serviceName} = \"{serviceName}\";{Environment.NewLine}");
                    File.WriteAllText(enumFilePath, enumContent, System.Text.Encoding.UTF8);
                }
            }
            else
            {
                throw new FileNotFoundException("Enum file not found", enumFilePath);
            }
            
            
        }
    }
}