using System.Threading.Tasks;

namespace GGS.Localization
{
    /// <summary>
    /// 语言数据加载器接口 - 支持多种加载方式（Resources、StreamingAssets、Addressables）
    /// </summary>
    public interface ILanguageDataLoader
    {
        /// <summary>
        /// 加载语言文件内容
        /// </summary>
        /// <param name="languageCode">语言代码</param>
        /// <returns>JSON 字符串</returns>
        Task<string> LoadLanguageFileAsync(string languageCode);

        /// <summary>
        /// 同步加载语言文件内容
        /// </summary>
        /// <param name="languageCode">语言代码</param>
        /// <returns>JSON 字符串</returns>
        string LoadLanguageFile(string languageCode);

        /// <summary>
        /// 检查语言文件是否存在
        /// </summary>
        /// <param name="languageCode">语言代码</param>
        /// <returns>是否存在</returns>
        bool LanguageFileExists(string languageCode);
    }
}
