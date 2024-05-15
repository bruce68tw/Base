using Base.Services;
using Ipfs;
using Ipfs.Commands;
using Pipelines.Sockets.Unofficial.Arenas;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace BaseEther.Services
{
#pragma warning disable IDE1006 // 命名樣式
    public static class _Ipfs
#pragma warning restore IDE1006 // 命名樣式
    {

        /// <summary>
        /// upload image to infura ipfs
        /// </summary>
        /// <param name="host">ex: https://ipfs.infura.io:5001</param>
        /// <param name="apiKey"></param>
        /// <param name="secretKey"></param>
        /// <param name="filePath"></param>
        /// <returns>ipfs hash, empty for error</returns>
        public static async Task<string> UploadImage(string host, string apiKey, string secretKey, string filePath)
        {
            //check file
            var error = "";
            if (!File.Exists(filePath))
            {
                error = "No File: " + filePath;
                goto lab_error;
            }

            //設定帳號密碼
            HttpClient client = new HttpClient();
            string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            try
            {
                using var ipfs = new IpfsClient(new Uri(host), client);
                //Wrap our stream in an IpfsStream, so it has a file name.
                var stream = new IpfsStream(filePath, File.OpenRead(filePath));
                var node = await ipfs.Add(stream);
                return node.Hash.ToString();
            }
            catch(Exception ex)
            {
                error = ex.Message;
                goto lab_error;
            }

        lab_error:
            _Log.Error("_Ipfs.cs UploadImage failed: " + error);
            return "";
        }

        public static async Task<string> UploadText(string host, string apiKey, string secretKey, string text)
        {
            //設定帳號密碼
            var error = "";
            HttpClient client = new HttpClient();
            string authValue = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{secretKey}"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authValue);

            try
            {
                using var ipfs = new IpfsClient(new Uri(host), client);
                byte[] textBytes = Encoding.UTF8.GetBytes(text);
                var stream = new IpfsStream("file.json", new MemoryStream(textBytes));
                var node = await ipfs.Add(stream);
                return node.Hash.ToString();
            }
            catch (Exception ex)
            {
                error = ex.Message;
                goto lab_error;
            }

        lab_error:
            _Log.Error("_Ipfs.cs UploadText failed: " + error);
            return "";
        }

    }//class
}