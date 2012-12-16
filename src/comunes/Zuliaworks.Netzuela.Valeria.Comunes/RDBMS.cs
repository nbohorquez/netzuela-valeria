﻿namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public delegate void ExpresionGenerica();

    /// <summary>
    /// SGBDR: Sistemas Gestores de Bases de Datos Relacionales (RDBMS en inglés)
    /// </summary>
    public static class RDBMS
    {
        public const string Netzuela = "Netzuela";          // No es realmente un SGBDR pero...
        public const string SqlServer = "SQL Server";
        public const string MySQL = "MySQL";
        public const string DbIsam = "DBISAM";
        public const string FireBird = "Firebird";
        public const string Predeterminado = "Predeterminado";
    }
}
