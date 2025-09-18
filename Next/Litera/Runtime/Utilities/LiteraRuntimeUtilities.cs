using System.Reflection;
using UnityEditorInternal;

namespace Next.Litera
{
    public static class LiteraRuntimeUtilities
    {
        public static void TransferMembersInfo(MemberInfo output, MemberInfo input, object outputObj, object inputObj)
        {
            SetMemberInfo(input, inputObj, GetMemberInfo(output, outputObj));
        }

        public static object GetMemberInfo(MemberInfo member, object obj)
        {
            PropertyInfo prop = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;

            if (prop == null && field == null) throw new System.Exception("Member must be property or field.");
            if (prop != null && !prop.CanRead) throw new System.Exception($"Property {prop.Name} of class {obj.GetType()} must contain getter.");

            return prop != null ? prop.GetValue(obj) : field.GetValue(obj);
        }

        public static void SetMemberInfo(MemberInfo member, object obj, object value)
        {
            PropertyInfo prop = member as PropertyInfo;
            FieldInfo field = member as FieldInfo;

            if (prop == null && field == null) throw new System.Exception("Member must be property or field.");
            if (prop != null && !prop.CanWrite) throw new System.Exception($"Property {prop.Name} of class {obj.GetType()} must contain setter.");

            if (prop != null) prop.SetValue(obj, value);
            else field.SetValue(obj, value);
        }

        public static string GetFilterable(this string s)
        {
            string res = "";
            foreach (char c in s) if (char.IsLetterOrDigit(c)) res += char.ToLower(c);
            return res;
        }
    }
}