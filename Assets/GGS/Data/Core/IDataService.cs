using System.Threading.Tasks;

namespace GGS.Data
{
    /// <summary>
    /// 数据服务接口 - 定义数据存取的基本操作
    /// </summary>
    public interface IDataService
    {
        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名（不含扩展名）</param>
        /// <returns>加载的数据实例，不存在时返回 null</returns>
        Task<T> LoadDataAsync<T>(string key) where T : class;

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">数据键名（不含扩展名）</param>
        /// <param name="data">要保存的数据实例</param>
        void SaveData<T>(string key, T data) where T : class;

        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <returns>是否成功删除</returns>
        bool DeleteData(string key);

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        /// <param name="key">数据键名</param>
        /// <returns>数据是否存在</returns>
        bool HasData(string key);

        /// <summary>
        /// 获取所有保存的数据键名
        /// </summary>
        /// <returns>数据键名数组</returns>
        string[] GetAllKeys();

        /// <summary>
        /// 清空所有数据
        /// </summary>
        void ClearAll();
    }
}