namespace Zuliaworks.Netzuela.Valeria.Comunes
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;     // RNGCryptoServiceProvider
    using System.Text;

    public class PasswordGenerator
    {
        /* 
         * Codigo importado
         * ================
         * 
         * Autor: Kevin Stewart
         * Titulo: A C# Password Generator 
         * Licencia: Ninguna
         * Fuente: http://www.codeproject.com/KB/cs/pwdgen.aspx
         * 
         * Tipo de uso
         * ===========
         * 
         * Textual                                              [X]
         * Adaptado                                             []
         * Solo se cambiaron los nombres de las variables       []
         * 
         */

        #region Variables

        private const int DefaultMinimum = 6;
        private const int DefaultMaximum = 10;
        private const int UBoundDigit = 61;

        private RNGCryptoServiceProvider rng;
        private int minSize;
        private int maxSize;
        private bool hasRepeating;
        private bool hasConsecutive;
        private bool hasSymbols;
        private string exclusionSet;

        /* 
         * Modifique pwdCharArray para que no incluya: coma (,), comillas dobles ("),  
         * comillas simples ('), vaina loca (`), punto y coma (;), dos puntos (:), 
         * barra (\), punto (.)
         */
        private char[] pwdCharArray = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789~!@#$%^&*()-_=+[]{}|<>/?".ToCharArray();

        #endregion

        #region Constructores

        public PasswordGenerator()
        {
            this.Minimum = DefaultMinimum;
            this.Maximum = DefaultMaximum;
            this.ConsecutiveCharacters = false;
            this.RepeatCharacters = true;
            this.ExcludeSymbols = false;
            this.Exclusions = null;

            this.rng = new RNGCryptoServiceProvider();
        }

        #endregion

        #region Propiedades

        public string Exclusions
        {
            get { return this.exclusionSet; }
            set { this.exclusionSet = value; }
        }

        public int Minimum
        {
            get 
            { 
                return this.minSize; 
            }

            set
            {
                this.minSize = value;
                if (PasswordGenerator.DefaultMinimum > this.minSize)
                {
                    this.minSize = PasswordGenerator.DefaultMinimum;
                }
            }
        }

        public int Maximum
        {
            get 
            { 
                return this.maxSize; 
            }

            set
            {
                this.maxSize = value;
                if (this.minSize >= this.maxSize)
                {
                    this.maxSize = PasswordGenerator.DefaultMaximum;
                }
            }
        }

        public bool ExcludeSymbols
        {
            get { return this.hasSymbols; }
            set { this.hasSymbols = value; }
        }

        public bool RepeatCharacters
        {
            get { return this.hasRepeating; }
            set { this.hasRepeating = value; }
        }

        public bool ConsecutiveCharacters
        {
            get { return this.hasConsecutive; }
            set { this.hasConsecutive = value; }
        }

        #endregion

        #region Funciones

        public string Generate()
        {
            // Pick random length between minimum and maximum   
            int pwdLength = this.GetCryptographicRandomNumber(this.Minimum, this.Maximum);

            StringBuilder pwdBuffer = new StringBuilder();
            pwdBuffer.Capacity = this.Maximum;

            // Generate random characters
            char lastCharacter, nextCharacter;

            // Initial dummy character flag
            lastCharacter = nextCharacter = '\n';

            for (int i = 0; i < pwdLength; i++)
            {
                nextCharacter = this.GetRandomCharacter();

                if (false == this.ConsecutiveCharacters)
                {
                    while (lastCharacter == nextCharacter)
                    {
                        nextCharacter = this.GetRandomCharacter();
                    }
                }

                if (false == this.RepeatCharacters)
                {
                    string temp = pwdBuffer.ToString();
                    int duplicateIndex = temp.IndexOf(nextCharacter);
                    while (-1 != duplicateIndex)
                    {
                        nextCharacter = this.GetRandomCharacter();
                        duplicateIndex = temp.IndexOf(nextCharacter);
                    }
                }

                if (null != this.Exclusions)
                {
                    while (-1 != this.Exclusions.IndexOf(nextCharacter))
                    {
                        nextCharacter = this.GetRandomCharacter();
                    }
                }

                pwdBuffer.Append(nextCharacter);
                lastCharacter = nextCharacter;
            }

            if (null != pwdBuffer)
            {
                return pwdBuffer.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        protected int GetCryptographicRandomNumber(int lowerBound, int upperBound)
        {
            // Assumes lBound >= 0 && lBound < uBound
            // returns an int >= lBound and < uBound
            uint urndnum;
            byte[] rndnum = new byte[4];
            if (lowerBound == upperBound - 1)
            {
                // test for degenerate case where only lBound can be returned   
                return lowerBound;
            }

            uint xcludeRndBase = uint.MaxValue - (uint.MaxValue % (uint)(upperBound - lowerBound));

            do
            {
                this.rng.GetBytes(rndnum);
                urndnum = System.BitConverter.ToUInt32(rndnum, 0);
            } 
            while (urndnum >= xcludeRndBase);

            return (int)(urndnum % (upperBound - lowerBound)) + lowerBound;
        }

        protected char GetRandomCharacter()
        {
            int upperBound = this.pwdCharArray.GetUpperBound(0);

            if (true == this.ExcludeSymbols)
            {
                upperBound = PasswordGenerator.UBoundDigit;
            }

            int randomCharPosition = this.GetCryptographicRandomNumber(this.pwdCharArray.GetLowerBound(0), upperBound);

            char randomChar = this.pwdCharArray[randomCharPosition];

            return randomChar;
        }

        #endregion
    }
}