using Amazon.S3;
using Amazon.S3.Model;
using MvcAWSApiConciertosMySql.Helpers;
using MvcAWSApiConciertosMySql.Models;
using Newtonsoft.Json;

namespace ProyectoTienda2.Services
{
    public class ServiceStorageS3
    {
        private string BucketName;
        private IAmazonS3 ClientS3;
        //SECRETO
        string miSecreto = HelperSecretManager.GetSecretAsync().Result;

        public ServiceStorageS3(IConfiguration configuration
            , IAmazonS3 clientS3)
        {
            KeysModel model = JsonConvert.DeserializeObject<KeysModel>(miSecreto);
            this.BucketName = configuration.GetValue<string>
                ("AWS:BucketName");
            this.ClientS3 = clientS3;
        }

        //COMENZAMOS SUBIENDO FICHEROS AL BUCKET
        //NECESITAMOS FileName, Stream y un Key/Value
        public async Task<bool>
            UploadFileAsync(string fileName, Stream stream)
        {
            PutObjectRequest request = new PutObjectRequest
            {
                InputStream = stream,
                Key = fileName,
                BucketName = this.BucketName
            };

            PutObjectResponse response = await
                this.ClientS3.PutObjectAsync(request);
            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<Stream> GetFileAsync(string fileName)
        {
            GetObjectResponse response =
                await this.ClientS3.GetObjectAsync(this.BucketName, fileName);
            return response.ResponseStream;
        }

    }
}
