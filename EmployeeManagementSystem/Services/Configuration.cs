using System.Xml;

namespace EmployeeManagementSystem.Services
{
    public class Configuration(IServiceCollection services)
    {
        private readonly IServiceCollection _services = services;

        public void SetUpServices()
        {
            _services.AddScoped<IEmployeeService, EmployeeService>();
            _services.AddScoped<IJwtTokenService, JwtTokenService>();
        }
    }
}
