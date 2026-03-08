using System;
using System.Security.Cryptography;
using System.Text;

namespace GGS.Data
{
    /// <summary>
    /// AES 加密服务实现
    /// 提供对称加密功能，保护敏感数据
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;
        private readonly bool _enabled;

        /// <summary>
        /// 是否启用加密
        /// </summary>
        public bool IsEnabled => _enabled;

        /// <summary>
        /// 创建 AES 加密服务
        /// </summary>
        /// <param name="key">加密密钥（32 字节，Base64 编码）</param>
        /// <param name="iv">初始化向量（16 字节，Base64 编码）</param>
        /// <param name="enabled">是否启用加密</param>
        public AesEncryptionService(string key = null, string iv = null, bool enabled = true)
        {
            _enabled = enabled;

            if (string.IsNullOrEmpty(key))
            {
                // 默认密钥（生产环境应从配置或服务器获取）
                key = "ThisIsA32ByteKeyForAES_Encryption!!";
            }

            if (string.IsNullOrEmpty(iv))
            {
                // 默认 IV
                iv = "InitVector_16Byt";
            }

            // 将字符串转换为固定长度的字节数组
            _key = new byte[32];
            _iv = new byte[16];

            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

            Array.Copy(keyBytes, _key, Math.Min(keyBytes.Length, 32));
            Array.Copy(ivBytes, _iv, Math.Min(ivBytes.Length, 16));
        }

        public string Encrypt(string plainText)
        {
            if (!_enabled || string.IsNullOrEmpty(plainText))
                return plainText;

            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (var msEncrypt = new System.IO.MemoryStream())
                    {
                        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (var swEncrypt = new System.IO.StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AesEncryptionService] 加密失败: {ex.Message}");
                return plainText;
            }
        }

        public string Decrypt(string cipherText)
        {
            if (!_enabled || string.IsNullOrEmpty(cipherText))
                return cipherText;

            try
            {
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = _iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    using (var msDecrypt = new System.IO.MemoryStream(cipherBytes))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (var srDecrypt = new System.IO.StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[AesEncryptionService] 解密失败: {ex.Message}");
                return cipherText;
            }
        }
    }
}
