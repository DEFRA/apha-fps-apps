using Apha.BatchJobs.Worker;

var services = ServiceCollectionSetup.CreateDefaultServices();

Console.WriteLine($"BatchJobs worker initialized with {services.Count} service registrations.");