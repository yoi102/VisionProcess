using System.Reflection;

namespace VisionProcess.Core.Extentions
{
    public class PropertyMisc
    {
        /// <summary>
        /// 获取类对应属性名称的属性信息
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static PropertyInfo? GetPropertyInfo(Type type, string propertyName)
        {
            string[] array = propertyName.Split('[', ']');//防止引锁问题
            return type.GetProperty(array.First());
        }

        /// <summary>
        /// 获取实例所对应的属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns></returns>
        public static object? GetPropertyValue(object instance, string propertyName)
        {
            ArgumentNullException.ThrowIfNull(instance);
            PropertyInfo? propertyInfo = GetPropertyInfo(instance.GetType(), propertyName);
            if (propertyInfo == null)
            {
                return null;
            }
            string[] array = propertyName.Split('[', ']');
            return propertyName.Contains('[')
                ? int.TryParse(array[1], out int index)
                    ? propertyInfo.GetValue(instance, [index])//
                    : propertyInfo.GetValue(instance, [array[1]])//若不是 int ，将是为 sting
                : propertyInfo.GetValue(instance);
        }

        public static Type? GetType(object instance, string fullPath, params char[] spiltChars)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(fullPath);

            string[] propertyNames = SplitFullPath(fullPath, spiltChars);
            if (propertyNames.Count() == 0)
            {
                return instance.GetType();
            }

            Type? type = instance.GetType();
            for (int i = 0; i < propertyNames.Count(); i++)
            {
                string[] array = propertyNames[i].Split('(', ')');
                if (propertyNames[i].Contains('(') && propertyNames[i].Contains(')'))//如有（）则为方法
                {
                    MethodInfo[] methods = type.GetMethods();
                    type = methods.First(x => x.Name == array[0]).ReturnType;//若找不到将抛异常
                }
                else//如没有（）则为属性
                {
                    var p = GetPropertyInfo(type, propertyNames[i]);
                    if (p is null)
                    {
                        return null;
                    }
                    type = p.PropertyType;
                }
            }
            return type;
        }

        /// <summary>
        /// 获取 instance 对象中 fullPath 位置的 Value
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="fullPath">全地址</param>
        /// <param name="spiltChars">全地址分割符号，如果 spiltChars 没有，默认点</param>
        /// <returns></returns>
        public static object? GetValue(object instance, string fullPath, params char[] spiltChars)
        {
            ArgumentNullException.ThrowIfNull(instance);
            ArgumentNullException.ThrowIfNull(fullPath);
            string[] propertyNames = SplitFullPath(fullPath, spiltChars);
            if (propertyNames.Count() == 0)
            {
                return instance;
            }
            object? targetValue = instance;

            for (int i = 0; i < propertyNames.Count(); i++)
            {
                if (targetValue is null)
                    return null;
                if (propertyNames[i].Contains('(') && propertyNames[i].Contains(')'))//若为方法
                {
                    targetValue = RunMethod(targetValue, propertyNames[i].Split('(')[0]);
                }
                else//若为属性
                {
                    targetValue = GetPropertyValue(targetValue, propertyNames[i]);
                }
            }

            return targetValue;
        }

        /// <summary>
        /// 执行无参方法，需确认是否有该方法否则抛异常
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <returns></returns>
        public static object? RunMethod(object instance, string methodName)
        {
            MethodInfo? method = instance.GetType().GetMethod(methodName);
            return method?.Invoke(instance, null);
        }

        /// <summary>
        /// 运行方法（带有形参，不允许含特殊修饰符，如 ref,out 等）
        /// </summary>
        /// <param name="instance">当前对象</param>
        /// <param name="methodName">对象的方法名</param>
        /// <param name="paramType">方法的参数类型</param>
        /// <param name="param">方法的参数</param>
        /// <returns></returns>
        public static object? RunMethod(object instance, string methodName, Type[] paramType, params object[] param)
        {
            MethodInfo? method = instance.GetType().GetMethod(methodName, paramType);
            return method?.Invoke(instance, param);
        }

        /// <summary>
        /// 获取 instance 中 fullPath 位置的Type
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        /// <summary>
        /// 设置实例所对应的属性的值
        /// </summary>
        /// <param name="instance">实例</param>
        /// <param name="propertyName">属性名称</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static bool TrySetPropertyValue(object instance, string propertyName, object? value)
        {
            ArgumentNullException.ThrowIfNull(instance);
            PropertyInfo? propertyInfo = GetPropertyInfo(instance.GetType(), propertyName);
            if (propertyInfo is null)
                return false;
            string[] array = propertyName.Split('[', ']');
            if (propertyName.Contains('['))
            {
                if (int.TryParse(array[1], out int index))
                {
                    propertyInfo.SetValue(instance, value, [index]);
                }
                else
                {
                    propertyInfo.SetValue(instance, value, [array[1]]);//若不是 int ，将是为 sting
                }
            }
            else
            {
                propertyInfo.SetValue(instance, value);
            }

            return true;
        }

        /// <summary>
        /// 设置 instance 对象中 fullPath 位置的Value
        /// </summary>
        /// <param name="instance">对象</param>
        /// <param name="fullPath">全地址</param>
        /// <param name="value">值</param>
        /// <param name="spiltChars">全地址分割符号，如果 spiltChars 没有，默认点</param>
        /// <returns></returns>
        public static bool TrySetValue(object instance, string fullPath, object? value, params char[] spiltChars)
        {
            if (instance == null)
            {
                return false;
            }
            object? targetInstance = instance;
            try
            {
                string[] propertyNames = SplitFullPath(fullPath, spiltChars);
                if (propertyNames.Count() == 0)
                {
                    return false;
                }
                for (int i = 0; i < propertyNames.Count(); i++)
                {
                    if (i == propertyNames.Count() - 1)
                    {
                        TrySetPropertyValue(targetInstance, propertyNames[i], value);
                    }
                    else
                    {
                        targetInstance = GetPropertyValue(targetInstance, propertyNames[i]);
                    }

                    if (targetInstance == null)
                        return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 分割 fullPath 的地址，
        /// 如果未设置 spiltChars ，默认点
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="spiltChars"></param>
        /// <returns></returns>
        private static string[] SplitFullPath(string fullPath, params char[] spiltChars)
        {
            string[] propertyNames = spiltChars.Length == 0 ? fullPath.Split('.') : fullPath.Split(spiltChars);   //使用.或者/来进行分割
            return propertyNames;
        }
    }
}