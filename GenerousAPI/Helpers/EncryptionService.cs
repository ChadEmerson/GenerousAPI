// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EncryptionService.cs" company="Day3 Solutions">
//   Copyright (c) Day3 Solutions. All rights reserved.
// </copyright>
// <summary>
//  Allows values to be encrypted and decrypted for secure storage
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace GenerousAPI.Helpers
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;

    public class EncryptionService
    {
        /// <summary>
        /// Password Hash value to use for encryption
        /// </summary>
        private const string PasswordHash = "788f5b8d-0493-4375-960c-0c032ce5edbc";

        /// <summary>
        /// Salt key to use for encryption
        /// </summary>
        private const string SaltKey = "12b394b7-53b9-4d7c-9f92-5498a0f262bb";

        /// <summary>
        /// Vector initialisation key for encryption
        /// </summary>
        private const string ViKey = "2@3B8FB8CE2D2B12"; //"@1B2c3D4e5F6g7H8"

        /// <summary>
        /// Encrypts a string of text using Rijndael along with the password hash value, salt key, and 
        /// vector initialisation key.
        /// </summary>
        /// <param name="plainText">Text to encrypt</param>
        /// <returns>Encrypted text</returns>
        public static string Encrypt(string plainText)
        {
            if (plainText == null)
                return null;

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(ViKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        /// <summary>
        /// Decrypts a string of text using the Rijndael method and the password hash value, salt key, and 
        /// vector initialisation key.
        /// </summary>
        /// <param name="encryptedText">Encrypted textto decrypt</param>
        /// <returns>Decrypted plain text </returns>
        public static string Decrypt(string encryptedText)
        {
            if (encryptedText == null)
                return null;

            byte[] cipherTextBytes = Convert.FromBase64String(encryptedText);
            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.None };

            var decryptor = symmetricKey.CreateDecryptor(keyBytes, Encoding.ASCII.GetBytes(ViKey));
            var memoryStream = new MemoryStream(cipherTextBytes);
            var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
            byte[] plainTextBytes = new byte[cipherTextBytes.Length];

            int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
            memoryStream.Close();
            cryptoStream.Close();
            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount).TrimEnd("\0".ToCharArray());
        }
    }
}