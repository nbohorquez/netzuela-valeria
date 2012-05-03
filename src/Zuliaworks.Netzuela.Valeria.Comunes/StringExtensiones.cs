namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class StringExtensiones
    {
        public static string CodificarBase64(this string data)
        {
            try
            {
                byte[] encbuff = System.Text.Encoding.UTF8.GetBytes(data);
                return Convert.ToBase64String(encbuff);
            }
            catch (Exception e)
            {
                throw new Exception("Error in CodificarBase64" + e.Message);
            }
        }

        public static string DecodificarBase64(this string data)
        {
            try
            {
                byte[] decbuff = Convert.FromBase64String(data);
                return System.Text.Encoding.UTF8.GetString(decbuff);
            }
            catch (Exception e)
            {
                throw new Exception("Error in DecodificarBase64" + e.Message);
            }
        }
    }
}
