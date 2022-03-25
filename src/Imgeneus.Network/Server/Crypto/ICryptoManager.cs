using System.Numerics;

namespace Imgeneus.Network.Server.Crypto
{
    public interface ICryptoManager
    {
        /// <summary>
        /// AES ctr decryption.
        /// </summary>
        /// <param name="encryptedBytes">encrypted bytes</param>
        /// <returns>decrypted bytes</returns>
        byte[] Decrypt(byte[] encryptedBytes);

        /// <summary>
        /// AES ctr encryption or xor encryption if character is in game.
        /// </summary>
        /// <param name="bytesToEnrypt">bytes we want to encrypt.</param>
        /// <returns>encrypted bytes</returns>
        byte[] Encrypt(byte[] bytesToEnrypt);

        /// <summary>
        /// Public exponent as little endian.
        /// </summary>
        byte[] RSAPublicExponent { get; }

        /// <summary>
        /// Modulus as little endian.
        /// </summary>
        byte[] RSAModulus { get; }

        /// <summary>
        /// Decrypts big int with rsa key. Pure rsa decryption, no padding.
        /// </summary>
        /// <param name="encrypted">encrypted big int</param>
        /// <returns>decrypted big int</returns>
        BigInteger DecryptRSA(BigInteger encrypted);

        /// <summary>
        /// Generates aes based on rsa decrypted number.
        /// Used only in login server.
        /// </summary>
        /// <param name="DecryptedMessage">big integer number, that we get from game.exe</param>
        void GenerateAES(BigInteger DecryptedMessage);

        /// <summary>
        /// Generates aes based on key and iv, that we get from login server.
        /// Used only in world server.
        /// </summary>
        /// <param name="key">bytes for key</param>
        /// <param name="iv">bytes for iv</param>
        void GenerateAES(byte[] key, byte[] iv);

        /// <summary>
        /// AES key.
        /// </summary>
        byte[] Key { get; }

        /// <summary>
        /// AES iv.
        /// </summary>
        byte[] IV { get; }

        /// <summary>
        /// In game it's used xor table (called expanded key)
        /// </summary>
        bool UseExpandedKey { get; set; }
    }
}
