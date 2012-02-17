namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security;      // SecureString
    using System.Text;

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

        private static byte[] entropia = Encoding.Unicode.GetBytes("Vos lo que sois es un muchacho");

        public static string Encriptar(this SecureString entrada)
        {
            byte[] encriptado = System.Security.Cryptography.ProtectedData.Protect(
                Encoding.Unicode.GetBytes(entrada.ConvertirAUnsecureString()),
                entropia,
                System.Security.Cryptography.DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encriptado);
        }

        public static SecureString Desencriptar(this string entrada)
        {
            try
            {
                byte[] desencriptado = System.Security.Cryptography.ProtectedData.Unprotect(
                    Convert.FromBase64String(entrada),
                    entropia,
                    System.Security.Cryptography.DataProtectionScope.CurrentUser);

                return System.Text.Encoding.Unicode.GetString(desencriptado).ConvertirASecureString();
            }
            catch
            {
                return new SecureString();
            }
        }
    }
}
