using Grpc.Core;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;

namespace ImagenService
{
    public class ImagenServiceImpl : ImagenService.ImagenServiceBase
    {
        private readonly IMongoDatabase _database;

        public ImagenServiceImpl(IMongoDatabase database)
        {
            _database = database;
        }

        public override async Task<UploadImageResponse> UploadImage(UploadImageRequest request, ServerCallContext context)
        {
            var collection = _database.GetCollection<BsonDocument>("autorImagenes");
            var document = new BsonDocument
            {
                { "autorLibroGuid", request.AutorLibroGuid },
                { "imagen", request.Imagen.ToByteArray() }
            };

            await collection.InsertOneAsync(document);

            return new UploadImageResponse
            {
                Message = "Imagen almacenada correctamente"
            };
        }

        public override async Task<GetAllImagesResponse> GetAllImages(GetAllImagesRequest request, ServerCallContext context)
        {
            var collection = _database.GetCollection<BsonDocument>("autorImagenes");
            var documents = await collection.Find(new BsonDocument()).ToListAsync();

            var response = new GetAllImagesResponse();
            foreach (var doc in documents)
            {
                var imageData = new ImageData
                {
                    AutorLibroGuid = doc["autorLibroGuid"].AsString,
                    Imagen = Google.Protobuf.ByteString.CopyFrom(doc["imagen"].AsByteArray)
                };
                response.Images.Add(imageData);
            }

            return response;
        }

        public override async Task<GetImageResponse> GetImage(GetImageRequest request, ServerCallContext context)
        {
            var collection = _database.GetCollection<BsonDocument>("autorImagenes");
            var filter = Builders<BsonDocument>.Filter.Eq("autorLibroGuid", request.AutorLibroGuid);
            var document = await collection.Find(filter).FirstOrDefaultAsync();

            var response = new GetImageResponse
            {
                AutorLibroGuid = request.AutorLibroGuid
            };

            if (document != null)
            {
                response.Imagen = Google.Protobuf.ByteString.CopyFrom(document["imagen"].AsByteArray);
            }
            else
            {
                response.Imagen = Google.Protobuf.ByteString.Empty;
            }

            return response;
        }
    }
}
