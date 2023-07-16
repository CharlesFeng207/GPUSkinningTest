using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PerfToolkit
{
    public class FunctionEvaluator
    {
        private enum SyntaxType
        {
            MethodCall,
            ValueSet,
            ValueGet
        }
        
        private readonly Dictionary<string, Type> m_TypeCache = new Dictionary<string, Type>();
        private readonly List<string> m_GlobalEnvironmentNameSpace = new List<string>()
        {
            "PerfToolkit",
            "UnityEngine"
        };

        public bool Execute(string code, out object returnObj)
        {
            returnObj = null;
            try
            {
                var parts = ParseCodePart(code);
                returnObj = FindRootType(ref parts);
                if (returnObj == null)
                    throw new Exception("Root type not found");
                
                while (parts.TryDequeue(out var p))
                {
                    Type targetType;
                    object targetInstance;

                    if (returnObj is Type type)
                    {
                        targetType = type;
                        targetInstance = null;
                    }
                    else
                    {
                        if (returnObj == null)
                            throw new Exception($"Target instance is null when executing {p}");

                        targetType = returnObj.GetType();
                        targetInstance = returnObj;
                    }

                    switch (CheckSyntaxType(p))
                    {
                        case SyntaxType.MethodCall:
                            returnObj = ExecuteMethodCall(targetType, targetInstance, p);
                            break;
                        case SyntaxType.ValueGet:
                            returnObj = ExecuteValueGet(targetType, targetInstance, p);
                            break;
                        case SyntaxType.ValueSet:
                            returnObj = ExecuteValueSet(targetType, targetInstance, p);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                returnObj = $"Error executing code: {code}\n{ex.Message}\n{ex.StackTrace}";
                return false;
            }

            return true;
        }

        private Queue<string> ParseCodePart(string code)
        {
            var parts = new Queue<string>();
            int j = 0;
            bool inBracket = false;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == ';')
                {
                    parts.Enqueue(code.Substring(j, i - j));
                    return parts;
                }
                
                if (code[i] == '(')
                {
                    inBracket = true;
                }
                else if (code[i] == ')')
                {
                    inBracket = false;
                }
                
                if (i == code.Length - 1)
                {
                    parts.Enqueue(code.Substring(j, code.Length - j));
                }
                else if (code[i] == '.' && !inBracket)
                {
                    parts.Enqueue(code.Substring(j, i - j));
                    j = i + 1;
                }
            }

            return parts;
        }

        private SyntaxType CheckSyntaxType(string code)
        {
            if (code.Contains("("))
                return SyntaxType.MethodCall;

            if (code.Contains("="))
                return SyntaxType.ValueSet;

            return SyntaxType.ValueGet;
        }

        private Type FindRootType(ref Queue<string> parts)
        {
            Assembly[] assemblies = null;
            var testParts = new Queue<string>(parts);
            var root = _FindRootType(ref testParts, ref assemblies, false);
            if (root == null)
            {
                testParts = new Queue<string>(parts);
                root = _FindRootType(ref testParts, ref assemblies, true);
            }
    
            parts = testParts;
            return root;
        }

        private Type _FindRootType(ref Queue<string> parts, ref Assembly[] assemblies, bool useGlobalNamespace)
        {
            string typeName = null;

            while (parts.TryDequeue(out var p))
            {
                typeName = string.IsNullOrEmpty(typeName) ? p : $"{typeName}.{p}";
                if (useGlobalNamespace)
                {
                    foreach (var globalNameSpace in m_GlobalEnvironmentNameSpace)
                    {
                        string testTypeName = $"{globalNameSpace}.{typeName}";
                        var root = TestType(testTypeName, ref assemblies);
                        if (root != null)
                            return root;        
                    }    
                }
                else
                {
                    var root = TestType(typeName, ref assemblies);
                    if (root != null)
                        return root;    
                }
            }

            return null;
        }
        
        private Type TestType(string typeName, ref Assembly[] assemblies)
        {
            var testTypeName = typeName;
                
            if (m_TypeCache.TryGetValue(testTypeName, out var value))
            {
                if (value != null)
                    return value;
            }

            if (assemblies == null) assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                var type = assembly.GetType(testTypeName);
                if (type != null)
                {
                    m_TypeCache[testTypeName] = type;
                    return type;
                }
            }

            m_TypeCache[testTypeName] = null;
            return null;
        }

        #region ValueGetSet

        private void ExtractSetStr(string code, out string targetStr, out string parameterStr)
        {
            int i = code.IndexOf('=');
            targetStr = code.Substring(0, i).Trim();
            parameterStr = code.Substring(i + 1).Trim();
        }

        private object ExecuteValueSet(Type targetType, object targetInstance, string code)
        {
            ExtractSetStr(code, out var targetStr, out string parameterStr);

            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.SetField | BindingFlags.SetProperty;
            PropertyInfo propertyInfo = targetType.GetProperty(targetStr, flags);
            if (propertyInfo != null)
            {
                propertyInfo.SetValue(targetInstance, Convert.ChangeType(parameterStr, propertyInfo.PropertyType));
                return null;
            }

            FieldInfo fieldInfo = targetType.GetField(targetStr, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(targetInstance, Convert.ChangeType(parameterStr, fieldInfo.FieldType));
                return null;
            }

            throw new Exception($"Can't find property or field {targetStr} in type {targetType}");
        }

        private object ExecuteValueGet(Type targetType, object targetInstance, string code)
        {
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                 BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.GetProperty;
            PropertyInfo propertyInfo = targetType.GetProperty(code, flags);
            if (propertyInfo != null)
            {
                return propertyInfo.GetValue(targetInstance);
            }

            FieldInfo fieldInfo = targetType.GetField(code, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(targetInstance);
            }

            throw new Exception($"Can't find property or field {code} in type {targetType}");
        }

        #endregion

        #region MethodCall

        private object ExecuteMethodCall(Type targetType, object targetInstance, string code)
        {
            ExtractFunctionCall(code, out var methodName, out var parameterStr);

            var methods = targetType.GetMethods();
            foreach (var method in methods)
            {
                if(method.Name != methodName)
                    continue;
                
                ParameterInfo[] parameterInfo = method.GetParameters();
                if(parameterInfo.Length != parameterStr.Length)
                    continue;
                
                object[] parameters = new object[parameterInfo.Length];
                
                try
                {
                    for (int i = 0; i < parameterInfo.Length; i++)
                    {
                        parameters[i] = Convert.ChangeType(parameterStr[i], parameterInfo[i].ParameterType);
                    }
                }
                catch (Exception e)
                {
                    continue;
                }
                
                return method.Invoke(targetInstance, parameters);    
            }
            
            throw new Exception($"Can't find method {targetType} {methodName} {parameterStr}");
        }

        private void ExtractFunctionCall(string code, out string methodName, out string[] parameter)
        {
            var startIndex = code.IndexOf('(');
            var endIndex = code.LastIndexOf(')');
            methodName = code.Substring(0, startIndex).Trim();
            var parameterStr = code.Substring(startIndex + 1, endIndex - startIndex - 1).Replace("\"", "").Trim();
            if (string.IsNullOrEmpty(parameterStr))
            {
                parameter = Array.Empty<string>();
            }
            else
            {
                parameter = parameterStr.Split(",").Select(x => x.Trim()).ToArray();
            }
        }

        #endregion
    }
}