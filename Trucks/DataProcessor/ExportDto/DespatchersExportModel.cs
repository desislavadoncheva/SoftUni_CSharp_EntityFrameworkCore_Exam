using System.Xml.Serialization;

namespace Trucks.DataProcessor.ExportDto
{
    [XmlType("Despatcher")]
    public class DespatchersExportModel
    {
        [XmlElement("DespatcherName")]        
        public string Name { get; set; }

        [XmlAttribute("TrucksCount")]
        public int TrucksCount { get; set; }

        [XmlArray("Trucks")]
        public ExportTrucksModel[] Trucks { get; set; }
    }

    [XmlType("Truck")]
    public class ExportTrucksModel
    {
        [XmlElement("RegistrationNumber")]
        public string RegistrationNumber { get; set; }

        [XmlElement("Make")]
        public string MakeType { get; set; }
    }
}
