namespace Trucks.DataProcessor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Trucks.DataProcessor.ExportDto;
    using Formatting = Newtonsoft.Json.Formatting;

    public class Serializer
    {
        public static string ExportDespatchersWithTheirTrucks(TrucksContext context)
        {
            StringBuilder sb = new StringBuilder();

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(DespatchersExportModel[]), new XmlRootAttribute("Despatchers"));

            XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
            namespaces.Add(string.Empty, string.Empty);

            using StringWriter sw = new StringWriter(sb);

            var despatchers = context
                .Despatchers
                .Where(d => d.Trucks.Any())
                .ToArray()
                .Select(d => new DespatchersExportModel()
                {
                    Name = d.Name,
                    TrucksCount = d.Trucks.Count,
                    Trucks = d.Trucks
                    .ToArray()
                    .Select(t => new ExportTrucksModel()
                    {
                        RegistrationNumber = t.RegistrationNumber,
                        MakeType = t.MakeType.ToString()
                    })
                    .OrderBy(t => t.RegistrationNumber)
                    .ToArray()
                })
            .OrderByDescending(d => d.TrucksCount)
            .ThenBy(d => d.Name)
            .ToArray();

            xmlSerializer.Serialize(sw, despatchers, namespaces);

            return sb.ToString().TrimEnd();
        }

        public static string ExportClientsWithMostTrucks(TrucksContext context, int capacity)
        {
            var clients = context.Clients.ToList()
                .Where(c => c.ClientsTrucks.Any(ct => ct.Truck.TankCapacity >= capacity))
                .ToArray()
                .Select(c => new
                 {
                    c.Name,
                    Trucks = c.ClientsTrucks
                        .Where(ct => ct.Truck.TankCapacity >= capacity)
                        .ToArray()                        
                        .Select(ct => new
                        {
                            TruckRegistrationNumber = ct.Truck.RegistrationNumber,
                            VinNumber = ct.Truck.VinNumber,
                            TankCapacity = ct.Truck.TankCapacity,
                            CargoCapacity = ct.Truck.CargoCapacity,
                            CategoryType = ct.Truck.CategoryType.ToString(),
                            MakeType = ct.Truck.MakeType.ToString()
                        })
                        .ToArray()
                        .OrderBy(ct => ct.MakeType)
                        .ThenBy(ct => ct.CargoCapacity)
                        .ToArray()
                })
                .OrderByDescending(ct => ct.Trucks.Count())
                .OrderBy(c => c.Name)
                .ToArray()
                .Take(10)
                .ToList();

            var result = JsonConvert.SerializeObject(clients, Formatting.Indented);
            return result;
        }
    }
}
