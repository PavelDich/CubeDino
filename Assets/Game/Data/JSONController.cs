using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Minicop.Game.CubeDino
{
    public static class JSONController
    {
        /// <summary>
        /// Сохранение обьекта, массивы внутри обьекта не сохраняються
        /// </summary>
        /// <param name="item">обьект сохранения</param>
        /// <param name="name">имя сохранения</param>
        public static void Save(object item, string name)
        {
#if PLATFORM_ANDROID
// Не все оболочки Android поддерживают json, поэтому использована PlayerPrefs
            PlayerPrefs.SetString(name, ToJson(item));
#else
            Directory.CreateDirectory(Application.dataPath);
            File.WriteAllText(Application.dataPath + $"/{name}.json", ToJson(item));
#endif
        }
        /// <summary>
        /// Загрузка обьекта
        /// </summary>
        /// <typeparam name="T">неявный тип обьекта </typeparam>
        /// <param name="item">обьект для перепеси на загруженное</param>
        /// <param name="name">имя загружаемого обьекта</param>
        public static void Load<T>(ref T item, string name)
        {
            try
            {
#if PLATFORM_ANDROID
                object obj = (object)item;
                obj = PlayerPrefs.GetString(name);
                item = (T)obj;
#else
                object obj = (object)item;
                FromJson(ref obj, File.ReadAllText(Application.dataPath + $"/{name}.json"));
                item = (T)obj;
#endif
            }
            catch { Save(item, name); }
        }

/// <summary>
/// Преоброзавнаие обьекта в JSON строку
/// </summary>
/// <param name="item">обьект преопрозования</param>
/// <returns></returns>
        public static string ToJson(object item)
        {
            return JsonUtility.ToJson(item);
        }

/// <summary>
/// Преоброзование в обьект из JSON строки
/// </summary>
        /// <typeparam name="T">неявный тип обьекта </typeparam>
        /// <param name="item">обьект для перепеси на загруженное</param>
        /// <param name="name">строка JSON</param>
        public static void FromJson<T>(ref T item, string json)
        {
            object obj = (object)item;
            JsonUtility.FromJsonOverwrite(json, obj);
            item = (T)obj;
        }
    }
}