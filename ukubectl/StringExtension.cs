using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ukubectl
{

    public static class StringExtension
    {
        public static SecureString ToSecureString(this string text)
        {
            SecureString secure = new SecureString();

            foreach (char ch in text)
            {
                secure.AppendChar(ch);
            }

            return secure;
        }
    }
}
