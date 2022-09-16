// See https://aka.ms/new-console-template for more information

using System.Drawing.Text;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Veiligstallen.BikeCounter.ApiClient;
using Veiligstallen.BikeCounter.ApiClient.DataModel;
using Veiligstallen.BikeCounter.ApiClient.DataModel.Converters;
using Veiligstallen.BikeCounter.ApiClient.Loader;

Console.WriteLine("VeiligStallen BikeCounter ApiClient Test :: v1");

var rn = Environment.NewLine;
var cfg = Cartomatic.Utils.NetCoreConfig.GetNetCoreConfig("credentials") //also load credentials cfg
    .GetSection("VeiligStallenBikeCounter")
    .Get<Configuration>();

var service = new Veiligstallen.BikeCounter.ApiClient.Service(cfg.User, cfg.Pass);

//await TestAuthAsync();

//TestGeomSerialization();

//await TestOrgApisAsync();


await CleanTestEnvAsync();

//await PrepareFlatDemoFiles();
await LoadFlatDemoFiles();

//await TestStaticDataLoaderAsync();



async Task TestOrgApisAsync()
{
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
}

async Task TestAuthAsync()
{
    Console.Write("Obtaining system roles... ");
    var authOutput = await service.AuthenticateAsync();
    Console.Write($"Done!{rn}");
    Console.WriteLine($"System roles: {JsonConvert.SerializeObject(authOutput)}");
    Console.WriteLine();
}

async Task CleanTestEnvAsync()
{
    Console.Write("Cleaning up test env... ");
    var envCleanedUp = await service.ResetEnvironmentAsync();
    Console.Write($"Done!{rn}");
    Console.WriteLine($"Test env cleaned up: {envCleanedUp}");
    Console.WriteLine();
}

async Task PrepareFlatDemoFiles()
{
    var dir = @"F:\OneDrive\OneDrive - Cartomatic Dominik Mikiewicz\Projects\Trajan\Trajan.Dashboard\_crow_data_upload";
    var dataLoader = new Veiligstallen.BikeCounter.ApiClient.Loader.StaticSurveyDataLoader(dir, extractWkt: true);

    await dataLoader.ExtractDataAsync();

    dataLoader.ExportFlatFiles(dir);
}

async Task LoadFlatDemoFiles()
{
    var surveyAreas = @"F:\OneDrive\OneDrive - Cartomatic Dominik Mikiewicz\Projects\Trajan\Trajan.Dashboard\_crow_data_upload\survey_areas.tsv";
    var parkingLocations = @"F:\OneDrive\OneDrive - Cartomatic Dominik Mikiewicz\Projects\Trajan\Trajan.Dashboard\_crow_data_upload\parking_locations.tsv";
    var sections = @"F:\OneDrive\OneDrive - Cartomatic Dominik Mikiewicz\Projects\Trajan\Trajan.Dashboard\_crow_data_upload\sections.tsv";

    Console.WriteLine("Loading data...");

    try
    {
        await service.ExtractAndUploadSurveyAreasFlatAsync(surveyAreas, FlatFileSeparator.Tab, true, (sender, msg) => { Console.WriteLine(msg); });

        await service.ExtractAndUploadParkingLocationsFlatAsync(parkingLocations, FlatFileSeparator.Tab, true, (sender, msg) => { Console.WriteLine(msg); });

        await service.ExtractAndUploadSectionsFlatAsync(sections, FlatFileSeparator.Tab, true, (sender, msg) => { Console.WriteLine(msg); });
    }
    catch (Exception ex)
    {
        Console.WriteLine("BOOM!");
        Console.WriteLine(ex.Message);
    }


    Console.WriteLine();
}

async Task TestStaticDataLoaderAsync()
{
    var dir = @"F:\OneDrive\OneDrive - Cartomatic Dominik Mikiewicz\Projects\Trajan\Trajan.Dashboard\_crow_data_upload";
    
    Console.WriteLine("Loading data...");

    try
    {
        await service.ExtractAndUploadStaticDataAsync(dir, (sender, msg) => { Console.WriteLine(msg); });
    }
    catch (Exception ex)
    {
        Console.WriteLine("BOOM!");
        Console.WriteLine(ex.Message);
    }
    

    Console.WriteLine();

}


void TestGeomSerialization()
{
    Console.Write("Testing geometry serialization... ");

    var dataFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "testGeoms.json");
    var data = File.ReadAllText(dataFile);
    
    var geoms = JsonConvert.DeserializeObject<List<TestObjectWithGeom>>(data);

    foreach (var geom in geoms)
    {
        Console.WriteLine(JsonConvert.SerializeObject(geom, Formatting.Indented));
        Console.WriteLine();
    }

    Console.WriteLine("Geometry serialization tests finished");
    Console.WriteLine();
}

class TestObjectWithGeom
{
    [JsonConverter(typeof(GeometryConverter))]
    public Geometry Location { get; set; }
}