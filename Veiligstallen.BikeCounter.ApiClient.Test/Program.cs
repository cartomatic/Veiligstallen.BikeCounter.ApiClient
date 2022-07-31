// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient;

Console.WriteLine("VeiligStallen BikeCounter ApiClient Test :: v1");

var rn = Environment.NewLine;
var cfg = Cartomatic.Utils.NetCoreConfig.GetNetCoreConfig("credentials") //also load credentials cfg
    .GetSection("VeiligStallenBikeCounter")
    .Get<Configuration>();

var service = new Veiligstallen.BikeCounter.ApiClient.Service(cfg.User, cfg.Pass);


Console.Write("Cleaning up test env... ");
var envCleanedUp = await service.ResetEnvironmentAsync();
Console.Write($"Done!{rn}");
Console.WriteLine($"Test env cleaned up: {envCleanedUp}");
Console.WriteLine();


Console.Write("Obtaining system roles... ");
var authOutput = await service.AuthenticateAsync();
Console.Write($"Done!{rn}");
Console.WriteLine($"System roles: {JsonConvert.SerializeObject(authOutput)}");
Console.WriteLine();


Console.Write("Getting organizations... ");
var orgs = await service.GetOrganizationsAsync();
Console.Write($"Done!{rn}");
Console.WriteLine($"Organizations: {JsonConvert.SerializeObject(orgs)}");
Console.WriteLine();

if (orgs.Any())
{
    Console.Write($"Getting organization {orgs.First().Id}... ");
    var org = await service.GetOrganizationAsync(orgs.First().Id);
    Console.Write($"Done!{rn}");
    Console.WriteLine($"Organization: {JsonConvert.SerializeObject(org)}");
    Console.WriteLine();
}
