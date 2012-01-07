using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security;      // SecureString

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    public static class Criptografia
    {
        // Con codigo de
        // http://weblogs.asp.net/jgalloway/archive/2008/04/13/encrypting-passwords-in-a-net-app-config-file.aspx

        static byte[] _Entropia = Encoding.Unicode.GetBytes("Vos lo que sois es un muchacho");

        public static string Encriptar(this SecureString Entrada)
        {
            byte[] Encriptado = System.Security.Cryptography.ProtectedData.Protect(
                Encoding.Unicode.GetBytes(Entrada.ConvertirAUnsecureString()),
                _Entropia,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(Encriptado);
        }

        public static SecureString Desencriptar(this string Entrada)
        {
            try
            {
                byte[] Desencriptado = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(Entrada),
                    _Entropia,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return System.Text.Encoding.Unicode.GetString(Desencriptado).ConvertirASecureString();
            }
            catch
            {
                return new SecureString();
            }
        }
    }
}
