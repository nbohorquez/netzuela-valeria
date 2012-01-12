using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Security;      // SecureString

namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    public static class Criptografia
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: Jon Galloway
         * Titulo: Encrypting Passwords in a .NET app.config File
         * Licencia: All posted code is published under Public Domain license unless otherwise stated. 
         *           All content is published under a Creative Commons Attribution License 
         * Fuente: http://weblogs.asp.net/jgalloway/archive/2008/04/13/encrypting-passwords-in-a-net-app-config-file.aspx
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              []
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       [X]
         * 
         */

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
