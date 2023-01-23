using System.Text;
using JurgenGraupeAssesment;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

class Program {    
    
    static void Main(string[] args) {
        // Read binary data file
        var kdTree = ReadBinaryDataFile("VehiclePositions.dat");        

        // Define the 10 coordinates to look up
        var coordinates = new Coordinate[] {
            new Coordinate(y: 34.544909, x: -102.100843),
            new Coordinate(y: 32.345544, x: -99.123124),
            new Coordinate(y: 33.234235, x: -100.214124),
            new Coordinate(y: 35.195739, x: -95.348899),
            new Coordinate(y: 31.895839, x: -97.789573),
            new Coordinate(y: 32.895839, x: -101.789573),
            new Coordinate(y: 34.115839, x: -100.225732),
            new Coordinate(y: 32.335839, x: -99.992232),
            new Coordinate(y: 33.535339, x: -94.792232),
            new Coordinate(y: 32.234235, x: -100.222222)            
        };

        // Find nearest vehicle position for each coordinate        
        foreach (var coord in coordinates) {            
            var nearest = kdTree.NearestNeighbor(coord);            
            Console.WriteLine($"Nearest vehicle to {coord.X}, {coord.Y}: \nPositionId: {nearest.Data.PositionId}\nRegistration: {nearest.Data.VehicleRegistration}\nLongitude: {nearest.Data.Longitude}\nLatitude: {nearest.Data.Latitude}\nTime: {nearest.Data.RecordedTimeUTC}\n\n");
        }        
    }

    public static KdTree<Vehicle> ReadBinaryDataFile(string filePath) {
        using (var reader = new BinaryReader(File.OpenRead(filePath))) {
            var kdTree = new KdTree<Vehicle>();
            while (reader.BaseStream.Position < reader.BaseStream.Length) {
                var positionId = reader.ReadInt32();
                var vehicleRegistration = ReadNullTerminatedString(reader);
                var latitude = reader.ReadSingle();
                var longitude = reader.ReadSingle();
                var recordedTimeUTC = reader.ReadUInt64();
                var vehicle = new Vehicle() {
                    PositionId = positionId,
                    VehicleRegistration = vehicleRegistration,
                    Latitude = latitude,
                    Longitude = longitude,
                    RecordedTimeUTC = recordedTimeUTC
                };
                kdTree.Insert(new Coordinate(vehicle.Longitude, vehicle.Latitude), vehicle);
            }
            return kdTree;
        }
    }

    public static string ReadNullTerminatedString(BinaryReader reader) {
        var bytes = new List<byte>();
        byte b;
        while ((b = reader.ReadByte()) != 0) {
            bytes.Add(b);
        }
        return Encoding.ASCII.GetString(bytes.ToArray());
    }    
}
