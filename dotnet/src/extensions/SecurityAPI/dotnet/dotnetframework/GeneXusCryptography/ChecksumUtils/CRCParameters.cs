using System.Security;


namespace GeneXusCryptography.ChecksumUtils
{
	[SecuritySafeCritical]
    public struct CRCParameters
    {
        private int _width;  
        public int Width
        {
            get => _width;
        }
        private long _polynomial; 
        public long Polynomial
        {
            get => _polynomial;
        }
        private bool _reflectIn; 
        public bool ReflectIn
        {
            get => _reflectIn;
        }
        private bool _reflectOut;   
        public bool ReflectOut
        {
            get => _reflectOut;
        }
        private long _init; 
        public long Init
        {
            get => _init;
        }
        private long _finalXor; 
        public long FinalXor
        {
            get => _finalXor;

        }

        [SecuritySafeCritical]
        public CRCParameters(int width, long polynomial, long init, bool reflectIn, bool reflectOut, long finalXor)
        {
            _width = width;
            _polynomial = polynomial;
            _reflectIn = reflectIn;
            _reflectOut = reflectOut;
            _init = init;
            _finalXor = finalXor;
        }

    }
}
