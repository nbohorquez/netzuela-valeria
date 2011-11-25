using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Configuration;

namespace Zuliaworks.Netzuela.Valeria.LogicaPresentacion.Configuraciones
{
    [ConfigurationCollection(typeof(MapeoDeColumnasConfig),
    CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class TablaMapeadaConfig : ConfigurationElementCollection
    {
        #region Variables

        private static ConfigurationPropertyCollection _Propiedades;

        #endregion

        #region Constructors

        static TablaMapeadaConfig()
        {
            _Propiedades = new ConfigurationPropertyCollection();
        }

        public TablaMapeadaConfig() { }

        #endregion

        #region Propiedades
        
        protected override ConfigurationPropertyCollection Properties
        {
            get { return _Propiedades; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.AddRemoveClearMap; }
        }

        public MapeoDeColumnasConfig this[int Indice]
        {
            get { return (MapeoDeColumnasConfig)base.BaseGet(Indice); }
            set
            {
                if (base.BaseGet(Indice) != null)
                {
                    base.BaseRemoveAt(Indice);
                }
                base.BaseAdd(Indice, value);
            }
        }

        public MapeoDeColumnasConfig this[string Nombre]
        {
            get { return (MapeoDeColumnasConfig)base.BaseGet(Nombre); }
        }

        #endregion

        #region Funciones

        protected override ConfigurationElement CreateNewElement()
        {
            return new MapeoDeColumnasConfig();
        }

        protected override object GetElementKey(ConfigurationElement Elemento)
        {
            return (Elemento as MapeoDeColumnasConfig).NodoOrigen;
        }

        public void Add(MapeoDeColumnasConfig Objeto)
        {
            base.BaseAdd(Objeto);
        }

        public void Remove(string name)
        {
            base.BaseRemove(name);
        }

        public void Remove(MapeoDeColumnasConfig Objeto)
        {
            base.BaseRemove(GetElementKey(Objeto));
        }

        public void Clear()
        {
            base.BaseClear();
        }

        public void RemoveAt(int Indice)
        {
            base.BaseRemoveAt(Indice);
        }

        public string GetKey(int Indice)
        {
            return (string)base.BaseGetKey(Indice);
        }

        #endregion
    }
}
