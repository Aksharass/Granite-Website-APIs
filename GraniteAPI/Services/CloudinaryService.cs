using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace GraniteAPI.Services
{
    public class CloudinaryService
    {
        private readonly Cloudinary _cloudinary;

        public CloudinaryService(IConfiguration config)
        {
            var cloudName = config["Cloudinary:CloudName"];
            var apiKey = config["Cloudinary:ApiKey"];
            var apiSecret = config["Cloudinary:ApiSecret"];

            _cloudinary = new Cloudinary(new Account(cloudName, apiKey, apiSecret));
            _cloudinary.Api.Secure = true;
        }

        public async Task<string> UploadBase64ImageAsync(string base64)
        {
            if (string.IsNullOrEmpty(base64))
                return null;

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription("base64image", base64),
                Folder = "granite-images"   // Cloudinary folder
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult.Error != null)
                throw new Exception(uploadResult.Error.Message);

            return uploadResult.SecureUrl.ToString(); // final image URL
        }
    }
}

