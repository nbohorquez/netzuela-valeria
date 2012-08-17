namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public static class Privilegios
    {
        public const int NoValido = 0;
        public const int Seleccionar = 1 << 0;
        public const int InsertarFilas = 1 << 1;
        public const int Actualizar = 1 << 2;
        public const int BorrarFilas = 1 << 3;
        public const int Indizar = 1 << 4;
        public const int Alterar = 1 << 5;
        public const int Crear = 1 << 6;
        public const int Destruir = 1 << 7;
    }
}
