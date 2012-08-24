namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class ManejadorDeExcepciones
    {
        public static string MostrarPilaDeExcepciones(this Exception ex)
        {
            List<string> Resultado = new List<string>();

            Resultado.Add("Pila de excepciones:\n");
            for (int i = 0; true; ex = ex.InnerException, i++)
            {
                Resultado.Add(" [excepcion_nivel" + i.ToString() + "] ");
                Resultado.Add(ex.Source + ".dll: \"" + ex.Message + "\"" + "\n");

                if (ex.InnerException == null)
                {
                    break;
                }
            }

            Resultado.Add("Pila de llamadas: \n" + ex.StackTrace);
            return string.Concat(Resultado.ToArray());
        }
    }
}
