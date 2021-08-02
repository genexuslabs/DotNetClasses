using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    public enum CanonicalizerWrapper
    {
        NONE, ALGO_ID_C14N_WITH_COMMENTS, ALGO_ID_C14N_OMIT_COMMENTS, ALGO_ID_C14N_EXCL_OMIT_COMMENTS, ALGO_ID_C14N_EXCL_WITH_COMMENTS
    }

    [SecuritySafeCritical]
    public class CanonicalizerWrapperUtils
    {
        public static CanonicalizerWrapper getCanonicalizerWrapper(string canonicalizerWrapper, Error error)
        {
            switch (canonicalizerWrapper.Trim())
            {
                case "C14n_WITH_COMMENTS":
                    return CanonicalizerWrapper.ALGO_ID_C14N_WITH_COMMENTS;
                case "C14n_OMIT_COMMENTS":
                    return CanonicalizerWrapper.ALGO_ID_C14N_OMIT_COMMENTS;
                case "exc_C14n_OMIT_COMMENTS":
                    return CanonicalizerWrapper.ALGO_ID_C14N_EXCL_OMIT_COMMENTS;
                case "exc_C14N_WITH_COMMENTS":
                    return CanonicalizerWrapper.ALGO_ID_C14N_EXCL_WITH_COMMENTS;
                default:
                    error.setError("CM001", "Unrecognized CanonicalizationMethod: " + canonicalizerWrapper);
                    return CanonicalizerWrapper.NONE;
            }
        }

        public static string valueOf(CanonicalizerWrapper canonicalizerWrapper, Error error)
        {
            switch (canonicalizerWrapper)
            {
                case CanonicalizerWrapper.ALGO_ID_C14N_WITH_COMMENTS:
                    return "C14n_WITH_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_OMIT_COMMENTS:
                    return "C14n_OMIT_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_OMIT_COMMENTS:
                    return "exc_C14n_OMIT_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_WITH_COMMENTS:
                    return "exc_C14N_WITH_COMMENTS";
                default:
                    error.setError("CM002", "Unrecognized CanonicalizationMethod");
                    return "";
            }
        }

        public static string valueOfInternal(CanonicalizerWrapper canonicalizerWrapper, Error error)
        {
            switch (canonicalizerWrapper)
            {
                case CanonicalizerWrapper.ALGO_ID_C14N_WITH_COMMENTS:
                    return "ALGO_ID_C14N_WITH_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_OMIT_COMMENTS:
                    return "ALGO_ID_C14N_OMIT_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_OMIT_COMMENTS:
                    return "ALGO_ID_C14N_EXCL_OMIT_COMMENTS";
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_WITH_COMMENTS:
                    return "ALGO_ID_C14N_EXCL_WITH_COMMENTS";
                default:
                    error.setError("CM003", "Unrecognized CanonicalizationMethod");
                    return "";
            }
        }

        public static string getCanonicalizationMethodAlorithm(CanonicalizerWrapper canonicalizerWrapper, Error error)
        {
            switch (canonicalizerWrapper)
            {
                case CanonicalizerWrapper.ALGO_ID_C14N_WITH_COMMENTS:
                    return Constants.ALGO_ID_C14N_WITH_COMMENTS;
                case CanonicalizerWrapper.ALGO_ID_C14N_OMIT_COMMENTS:
                    return Constants.ALGO_ID_C14N_OMIT_COMMENTS;
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_OMIT_COMMENTS:
                    return Constants.ALGO_ID_C14N_EXCL_OMIT_COMMENTS;
                case CanonicalizerWrapper.ALGO_ID_C14N_EXCL_WITH_COMMENTS:
                    return Constants.ALGO_ID_C14N_EXCL_WITH_COMMENTS;
                default:
                    error.setError("CM004", "Unrecognized CanonicalizationMethod");
                    return null;

            }
        }




        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="CanonicalizerWrapper">AsymmetricSigningAlgorithm enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<CanonicalizerWrapper> GetValues<CanonicalizerWrapper>()
        {
            return Enum.GetValues(typeof(CanonicalizerWrapper)).Cast<CanonicalizerWrapper>();
        }
    }


}
