using JurgenGraupeAssesment;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using System.Text;

class Program {
    static void Main(string[] args) {
        var kdTree = ReadBinaryDataFile("VehiclePositions.dat");
        
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
        
        Parallel.ForEach(coordinates, coord =>  {
            var nearest = kdTree.NearestNeighbor(coord);
            Console.WriteLine($"Nearest vehicle to {coord.X}, {coord.Y}: \nPositionId: {nearest.Data.PositionId}\nRegistration: {nearest.Data.VehicleRegistration}\nLongitude: {nearest.Data.Longitude}\nLatitude: {nearest.Data.Latitude}\nTime: {nearest.Data.RecordedTimeUTC}\n\n");
        });
    }

    public static KdTree<Vehicle> ReadBinaryDataFile(string filePath) {
        var kdTree = new KdTree<Vehicle>();
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var reader = new BinaryReader(new BufferedStream(fileStream))) {
            int chunkSize = 4096;
            var buffer = new byte[chunkSize];
            int bytesRead;
            while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0) {
                using (var stream = new MemoryStream(buffer, 0, bytesRead))
                using (var binaryReader = new BinaryReader(stream)) {
                    try {
                        while (stream.Position < stream.Length) {
                            var positionId = binaryReader.ReadInt32();
                            var vehicleRegistration = ReadNullTerminatedString(binaryReader);
                            var latitude = binaryReader.ReadSingle();
                            var longitude = binaryReader.ReadSingle();
                            var recordedTimeUTC = binaryReader.ReadUInt64();
                            var vehicle = new Vehicle() {
                                PositionId = positionId,
                                VehicleRegistration = vehicleRegistration,
                                Latitude = latitude,
                                Longitude = longitude,
                                RecordedTimeUTC = recordedTimeUTC
                            };
                            kdTree.Insert(new Coordinate(vehicle.Longitude, vehicle.Latitude), vehicle);
                        }
                    } catch (EndOfStreamException) {
                        break;
                    }
                }
            }
        }
        return kdTree;
    }

    private static string ReadNullTerminatedString(BinaryReader reader) {
        List<byte> stringBytes = new List<byte>();
        byte b;
        while ((b = reader.ReadByte()) != 0) {
            stringBytes.Add(b);
        }
        return Encoding.UTF8.GetString(stringBytes.ToArray());
    }    
}
