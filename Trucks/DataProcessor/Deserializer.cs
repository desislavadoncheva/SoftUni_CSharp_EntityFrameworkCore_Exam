namespace Trucks.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Data;
    using Newtonsoft.Json;
    using Trucks.Data.Models;
    using Trucks.Data.Models.Enums;
    using Trucks.DataProcessor.ImportDto;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedDespatcher
            = "Successfully imported despatcher - {0} with {1} trucks.";

        private const string SuccessfullyImportedClient
            = "Successfully imported client - {0} with {1} trucks.";

        public static string ImportDespatcher(TrucksContext context, string xmlString)
        {
            var sb = new StringBuilder();
            XmlSerializer serializer = new XmlSerializer(typeof(DespatcherTrucksInputModel[]), new XmlRootAttribute("Despatchers"));
            TextReader reader = new StringReader(xmlString);
            var despatchers = (IEnumerable<DespatcherTrucksInputModel>)serializer.Deserialize(reader);

            foreach (var despatcherItem in despatchers)
            {
                if (!IsValid(despatcherItem))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                if (String.IsNullOrEmpty(despatcherItem.Position))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                };
                if (despatcherItem.Position == null)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                };
                var despatcher = new Despatcher()
                {
                    Name = despatcherItem.Name,
                    Position = despatcherItem.Position
                };
                foreach (TruckInputModel truckItem in despatcherItem.Trucks)
                {
                    if (!IsValid(truckItem))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }                    
                    var truck = new Truck()
                    {
                        RegistrationNumber = truckItem.RegistrationNumber,
                        VinNumber = truckItem.VinNumber,
                        TankCapacity = truckItem.TankCapacity,
                        CargoCapacity = truckItem.CargoCapacity,
                        CategoryType = (CategoryType) truckItem.CategoryType,
                        MakeType = (MakeType) truckItem.MakeType
                    };
                    despatcher.Trucks.Add(truck);
                }
                context.Despatchers.Add(despatcher);
                context.SaveChanges();
                sb.AppendLine(string.Format(SuccessfullyImportedDespatcher, despatcher.Name, despatcher.Trucks.Count));
            }
            return sb.ToString().TrimEnd();
    }
    public static string ImportClient(TrucksContext context, string jsonString)
        {
            var sb = new StringBuilder();
            var clients = JsonConvert.DeserializeObject<IEnumerable<ClientsInputModel>>(jsonString);
            foreach (var clientItem in clients)
            {
                if (!IsValid(clientItem))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                };
                if (clientItem.Type == "usual")
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                };
                var client = new Client()
                {
                    Name = clientItem.Name,
                    Nationality = clientItem.Nationality,
                    Type = clientItem.Type
                };
                foreach (var truckItem in clientItem.Trucks.Distinct())
                {
                    var truck = context.Trucks.Find(truckItem);
                    if (truck == null)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    };
                    var clientTruck = new ClientTruck()
                    {
                        Truck = truck
                    };
                    client.ClientsTrucks.Add(clientTruck);
                }
                context.Clients.Add(client);
                context.SaveChanges();
                sb.AppendLine(string.Format(SuccessfullyImportedClient, client.Name, client.ClientsTrucks.Count));
            }
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
