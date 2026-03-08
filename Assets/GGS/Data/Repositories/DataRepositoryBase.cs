using System;
using System.Threading.Tasks;
using UnityEngine;

namespace GGS.Data
{
    /// <summary>
    /// 数据仓库基类
    /// 封装数据访问逻辑，提供缓存和自动保存功能
    /// </summary>
    /// <typeparam name="T">数据类型</typeparam>
    public abstract class DataRepositoryBase<T> where T : class, new()
    {
        protected readonly IDataService _dataService;
        protected T _cachedData;
        protected readonly string _fileName;
        protected bool _isLoaded;
        protected bool _autoSave = true;

        /// <summary>
        /// 是否已加载数据
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 是否自动保存（数据变更时自动保存）
        /// </summary>
        public bool AutoSave
        {
            get => _autoSave;
            set => _autoSave = value;
        }

        /// <summary>
        /// 创建数据仓库
        /// </summary>
        /// <param name="dataService">数据服务</param>
        /// <param name="fileName">文件名（不含扩展名）</param>
        protected DataRepositoryBase(IDataService dataService, string fileName)
        {
            _dataService = dataService ?? throw new ArgumentNullException(nameof(dataService));
            _fileName = string.IsNullOrEmpty(fileName) ? typeof(T).Name : fileName;
        }

        /// <summary>
        /// 异步加载数据
        /// </summary>
        /// <returns>加载的数据实例</returns>
        public async Task<T> LoadAsync()
        {
            T data = await _dataService.LoadDataAsync<T>(_fileName);

            if (data == null)
            {
                // 数据不存在，创建新实例
                data = CreateNewInstance();
            }

            // 如果数据支持版本迁移，执行迁移
            if (data is IVersionMigratable migratable)
            {
                migratable.SetDefaults();

                if (migratable.NeedsMigration())
                {
                    migratable.Migrate();
                    // 迁移后需要保存
                    SaveInternal(data);
                }
            }
            else if (data is SaveDataBase saveData)
            {
                saveData.SetDefaults();
            }

            _cachedData = data;
            _isLoaded = true;

            OnDataLoaded(data);

            return data;
        }

        /// <summary>
        /// 同步加载数据（非异步版本）
        /// </summary>
        /// <returns>加载的数据实例</returns>
        public T Load()
        {
            // 在 Unity 主线程上运行异步任务
            T result = null;
            bool isComplete = false;

            LoadAsync().ContinueWith(t =>
            {
                result = t.Result;
                isComplete = true;
            });

            // 简单等待（实际项目中应使用异步模式）
            while (!isComplete)
            {
                System.Threading.Thread.Sleep(1);
            }

            return result;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void Save()
        {
            if (!_isLoaded || _cachedData == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 数据未加载，无法保存");
                return;
            }

            SaveInternal(_cachedData);
        }

        /// <summary>
        /// 获取数据（必须先调用 Load）
        /// </summary>
        /// <returns>当前缓存的数据</returns>
        /// <exception cref="InvalidOperationException">数据未加载时抛出</exception>
        public T Get()
        {
            if (!_isLoaded || _cachedData == null)
            {
                throw new InvalidOperationException($"[{GetType().Name}] 数据未加载，请先调用 Load 或 LoadAsync");
            }

            return _cachedData;
        }

        /// <summary>
        /// 尝试获取数据
        /// </summary>
        /// <param name="data">输出数据</param>
        /// <returns>是否成功获取</returns>
        public bool TryGet(out T data)
        {
            data = default;
            if (_isLoaded && _cachedData != null)
            {
                data = _cachedData;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 检查数据是否存在
        /// </summary>
        /// <returns>数据文件是否存在</returns>
        public bool Exists()
        {
            return _dataService.HasData(_fileName);
        }

        /// <summary>
        /// 删除数据
        /// </summary>
        public void Delete()
        {
            _dataService.DeleteData(_fileName);
            _cachedData = null;
            _isLoaded = false;

            OnDataDeleted();
        }

        /// <summary>
        /// 重置数据为默认值并保存
        /// </summary>
        public void Reset()
        {
            _cachedData = CreateNewInstance();
            Save();
            OnDataReset();
        }

        /// <summary>
        /// 修改数据（自动保存）
        /// </summary>
        /// <param name="modifier">修改回调</param>
        public void Modify(Action<T> modifier)
        {
            if (!_isLoaded || _cachedData == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 数据未加载，无法修改");
                return;
            }

            modifier?.Invoke(_cachedData);

            if (_autoSave)
            {
                Save();
            }
        }

        /// <summary>
        /// 修改数据（带返回值）
        /// </summary>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="modifier">修改回调</param>
        /// <returns>回调返回值</returns>
        public TResult Modify<TResult>(Func<T, TResult> modifier)
        {
            if (!_isLoaded || _cachedData == null)
            {
                Debug.LogWarning($"[{GetType().Name}] 数据未加载，无法修改");
                return default;
            }

            TResult result = modifier.Invoke(_cachedData);

            if (_autoSave)
            {
                Save();
            }

            return result;
        }

        /// <summary>
        /// 创建新数据实例
        /// 子类可覆盖以提供自定义初始化逻辑
        /// </summary>
        /// <returns>新数据实例</returns>
        protected virtual T CreateNewInstance()
        {
            T instance = new T();

            // 如果是 SaveDataBase，会自动初始化时间戳
            if (instance is SaveDataBase saveData)
            {
                saveData.SetDefaults();
            }

            OnDataCreated(instance);
            return instance;
        }

        /// <summary>
        /// 保存数据到存储
        /// </summary>
        private void SaveInternal(T data)
        {
            // 更新修改时间
            if (data is SaveDataBase saveData)
            {
                saveData.Touch();
            }
            else if (data is IVersionMigratable migratable)
            {
                migratable.Version = migratable.Version;
            }

            // 验证数据
            if (data is SaveDataBase saveDataBase && !saveDataBase.Validate())
            {
                Debug.LogWarning($"[{GetType().Name}] 数据验证失败");
            }

            _dataService.SaveData(_fileName, data);
            OnDataSaved(data);
        }

        #region 生命周期回调

        /// <summary>
        /// 数据加载完成后调用
        /// </summary>
        /// <param name="data">加载的数据</param>
        protected virtual void OnDataLoaded(T data) { }

        /// <summary>
        /// 数据保存完成后调用
        /// </summary>
        /// <param name="data">保存的数据</param>
        protected virtual void OnDataSaved(T data) { }

        /// <summary>
        /// 数据删除后调用
        /// </summary>
        protected virtual void OnDataDeleted() { }

        /// <summary>
        /// 数据重置后调用
        /// </summary>
        protected virtual void OnDataReset() { }

        /// <summary>
        /// 新数据创建后调用
        /// </summary>
        /// <param name="data">创建的数据</param>
        protected virtual void OnDataCreated(T data) { }

        #endregion
    }
}
