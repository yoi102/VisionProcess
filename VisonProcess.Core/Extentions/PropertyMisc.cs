using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VisonProcess.Core.Extentions
{
    //待修改
    public class PropertyMisc
    {
        private static List<string> SplitFullPath(string fullPath, params char[] spiltChars)
        {
            string[] array = ((spiltChars.Length == 0) ? fullPath.Split('.', '\\', '/') : fullPath.Split(spiltChars));
            List<string> list = new List<string>();
            bool flag = false;
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].Contains("(") && array[i].Contains(")"))
                {
                    list.Add(array[i]);
                }
                else if (array[i].Contains("("))
                {
                    flag = true;
                    list.Add(array[i]);
                }
                else if (flag && array[i].Contains(")"))
                {
                    flag = false;
                    List<string> list2 = list;
                    int index = list.Count - 1;
                    list2[index] = list2[index] + "." + array[i];
                }
                else if (flag)
                {
                    List<string> list2 = list;
                    int index = list.Count - 1;
                    list2[index] = list2[index] + "." + array[i];
                }
                else
                {
                    list.Add(array[i]);
                }
            }

            return list;
        }

        public static object GetValue(object ob, string fullPath, params char[] spiltChars)
        {
            List<string> list = SplitFullPath(fullPath, spiltChars);
            if (list.Count == 0)
            {
                return ob;
            }

            for (int i = 0; i < list.Count; i++)
            {
                string[] array = list[i].Split('[', ']', '<', '>', '(', '=');
                if (list[i].Contains("(") && list[i].Contains(")"))
                {
                    if (list[i].IndexOf('(') != 0)
                    {
                        try
                        {
                            ob = RunMethod(ob, array[0]);
                        }
                        catch
                        {
                            ob = null;
                        }

                        if (ob == null)
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    ob = GetPropertyValue(ob, list[i]);
                    if (ob == null)
                    {
                        return null;
                    }
                }
            }

            return ob;
        }

        public static bool SetValue(object ob, string fullPath, object value, params char[] spiltChars)
        {
            List<string> list = SplitFullPath(fullPath, spiltChars);
            if (list.Count == 0)
            {
                return false;
            }

            try
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Contains("("))
                    {
                        if (i == list.Count - 1)
                        {
                            SetPropertyValue(ob, list[i], value);
                        }
                        else
                        {
                            ob = GetPropertyValue(ob, list[i]);
                        }

                        if (ob == null)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static Type GetType(object ob, string fullPath, params char[] spiltChars)
        {
            List<string> list = SplitFullPath(fullPath, spiltChars);
            if (list.Count == 0)
            {
                return typeof(object);
            }

            Type type = ob.GetType();
            for (int i = 0; i < list.Count; i++)
            {
                string[] array = list[i].Split('[', ']', '<', '>', '(', ')', '=');
                if (list[i].Contains("(") && list[i].Contains(")"))
                {
                    if (list[i].IndexOf('(') == 0)
                    {
                        type = GetType(array[1]);
                        continue;
                    }

                    MethodInfo[] methods = type.GetMethods();
                    MethodInfo methodInfo = null;
                    MethodInfo[] array2 = methods;
                    foreach (MethodInfo methodInfo2 in array2)
                    {
                        if (methodInfo2.Name == array[0])
                        {
                            methodInfo = methodInfo2;
                            break;
                        }
                    }

                    type = methodInfo.ReturnType;
                }
                else
                {
                    PropertyInfo propertyInfo = GetPropertyInfo(type, array[0]);
                    if (propertyInfo == null)
                    {
                        return typeof(object);
                    }

                    type = propertyInfo.PropertyType;
                }
            }

            return type;
        }

        public static Type GetType(string typeName)
        {
            Type type = null;
            List<Assembly> list = AppDomain.CurrentDomain.GetAssemblies().ToList();
            List<string> list2 = Directory.EnumerateFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll").ToList();
            foreach (string item2 in list2)
            {
                if (!File.Exists(item2) || Path.GetFileName(item2) == "Cognex.VisionPro.Interop.Core.dll")
                {
                    continue;
                }

                try
                {
                    Assembly item = Assembly.LoadFile(item2);
                    if (!list.Contains(item))
                    {
                        list.Add(item);
                    }
                }
                catch
                {
                }
            }

            string[] array = typeName.Split(',');
            if (array.Length == 1)
            {
                int count = list.Count;
                for (int i = 0; i < count; i++)
                {
                    try
                    {
                        type = list[i].GetType(typeName);
                        if (type != null)
                        {
                            return type;
                        }

                        Type[] types = list[i].GetTypes();
                        int num = types.Length;
                        for (int j = 0; j < num; j++)
                        {
                            if (types[j].Name.Equals(typeName))
                            {
                                return types[j];
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else
            {
                Assembly assembly = null;
                int k = 0;
                for (int count2 = list.Count; k < count2; k++)
                {
                    if (list[k].FullName.Split(',')[0].Trim() == array[1].Trim())
                    {
                        assembly = list[k];
                        break;
                    }
                }

                if (assembly != null)
                {
                    type = assembly.GetType(array[0]);
                }
            }

            return type;
        }

        public static List<Type> GetTypes(string path, Predicate<Type> match)
        {
            List<Type> list = new List<Type>();
            path = Path.GetFullPath(path);
            Assembly assembly = Assembly.LoadFile(path);
            Type[] types = assembly.GetTypes();
            Type[] array = types;
            foreach (Type type in array)
            {
                if (match(type))
                {
                    list.Add(type);
                }
            }

            return list;
        }

        public static PropertyInfo GetPropertyInfo(Type type, string propertyName)
        {
            string[] array = propertyName.Split('[', ']', '<', '>', '(', '=');
            PropertyInfo result = null;
            try
            {
                result = type.GetProperty(array[0]);
            }
            catch (AmbiguousMatchException)
            {
                if (propertyName.Contains("["))
                {
                    bool flag = true;
                    int result2 = 0;
                    bool flag2 = int.TryParse(array[1], out result2);
                    flag = flag2;
                    PropertyInfo[] properties = type.GetProperties();
                    PropertyInfo[] array2 = properties;
                    foreach (PropertyInfo propertyInfo in array2)
                    {
                        if (propertyInfo.Name == array[0])
                        {
                            string text = propertyInfo.ToString();
                            if (propertyInfo.ToString().Contains("System.String") && !flag)
                            {
                                result = propertyInfo;
                                break;
                            }

                            if (propertyInfo.ToString().Contains("Int32") && flag)
                            {
                                result = propertyInfo;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    result = type.GetProperty(array[0], BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
                }
            }

            return result;
        }

        public static object GetPropertyValue(object ob, string propertyName)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(ob.GetType(), propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            string[] array = propertyName.Split('[', ']', '<', '>', '(', '=');
            if (propertyName.Contains("["))
            {
                int result = 0;
                if (int.TryParse(array[1], out result))
                {
                    int num = int.Parse(array[1]);
                    ob = propertyInfo.GetValue(ob, new object[1] { num });
                }
                else
                {
                    ob = propertyInfo.GetValue(ob, new object[1] { array[1] });
                }
            }
            else
            {
                ob = propertyInfo.GetValue(ob, null);
            }

            return ob;
        }

        public static bool SetPropertyValue(object ob, string propertyName, object objValue)
        {
            PropertyInfo propertyInfo = GetPropertyInfo(ob.GetType(), propertyName);
            if (propertyInfo == null)
            {
                return false;
            }

            string[] array = propertyName.Split('[', ']', '<', '>', '(', '=');
            if (propertyName.Contains("["))
            {
                int result = 0;
                if (int.TryParse(array[1], out result))
                {
                    int num = int.Parse(array[1]);
                    propertyInfo.SetValue(ob, objValue, new object[1] { num });
                }
                else
                {
                    propertyInfo.SetValue(ob, objValue, new object[1] { array[1] });
                }
            }
            else
            {
                propertyInfo.SetValue(ob, objValue);
            }

            return true;
        }

        public static object RunMethod(object ob, string methodName)
        {
            try
            {
                MethodInfo method = ob.GetType().GetMethod(methodName, Type.EmptyTypes);
                return method.Invoke(ob, null);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
        }

        public static object RunMethod(object ob, string methodName, Type[] paramType, params object[] param)
        {
            try
            {
                MethodInfo method = ob.GetType().GetMethod(methodName, paramType);
                return method.Invoke(ob, param);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

}
