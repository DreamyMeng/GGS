namespace GGS.Data
{
    /// <summary>
    /// JSON 序列化器接口 - 支持多种 JSON 库实现
    /// </summary>
    public interface IJsonSerializer
    {
        /// <summary>
        /// 将对象序列化为 JSON 字符串
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>JSON 字符串</returns>
        string Serialize(object obj);

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="json">JSON 字符串</param>
        /// <returns>反序列化后的对象</returns>
        T Deserialize<T>(string json);

        /// <summary>
        /// 将 JSON 字符串反序列化为对象
        /// </summary>
        /// <param name="json">JSON 字符串</param>
        /// <param name="type">目标类型</param>
        /// <returns>反序列化后的对象</returns>
        object Deserialize(string json, System.Type type);
    }
}
