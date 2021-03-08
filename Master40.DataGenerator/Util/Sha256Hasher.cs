using System.Security.Cryptography;
using System.Text;

namespace Master40.DataGenerator.Util
{
    public class Sha256Hasher
    {
        /**
         * taken from https://www.c-sharpcorner.com/article/compute-sha256-hash-in-c-sharp/ (2021-02-27)
         */
        public static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using SHA256 sha256Hash = SHA256.Create();
            // ComputeHash - returns byte array  
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

            // Convert byte array to a string   
            StringBuilder builder = new StringBuilder();
            foreach (var b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}