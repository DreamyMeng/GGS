namespace GGS.Data
{
    /// <summary>
    /// 加密服务接口 - 用于敏感数据的加密存储
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plainText">明文</param>
        /// <returns>加密后的 Base64 字符串</returns>
        string Encrypt(string plainText);

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="cipherText">加密的 Base64 字符串</param>
        /// <returns>明文</returns>
        string Decrypt(string cipherText);

        /// <summary>
        /// 是否启用加密
        /// </summary>
        bool IsEnabled { get; }
    }
}
