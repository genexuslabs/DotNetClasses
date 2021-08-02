﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using SecurityAPICommons.Commons;

namespace GeneXusXmlSignature.GeneXusUtils
{
    [SecuritySafeCritical]
    public enum TransformsWrapper
    {
        NONE, ENVELOPED, ENVELOPING, DETACHED,
    }

    [SecuritySafeCritical]
    public class TransformsWrapperUtils
    {
        public static TransformsWrapper getTransformsWrapper(string transformsWrapper, Error error)
        {
            switch (transformsWrapper.ToUpper().Trim())
            {
                case "ENVELOPED":
                    return TransformsWrapper.ENVELOPED;
                case "ENVELOPING":
                    return TransformsWrapper.ENVELOPING;
                case "DETACHED":
                    return TransformsWrapper.DETACHED;
                default:
                    error.setError("TW001", "Unrecognized transformation: " + transformsWrapper);
                    return TransformsWrapper.NONE;
            }
        }


        public static string valueOf(TransformsWrapper transformsWrapper, Error error)
        {
            switch (transformsWrapper)
            {
                case TransformsWrapper.ENVELOPED:
                    return "ENVELOPED";
                case TransformsWrapper.ENVELOPING:
                    return "ENVELOPING";
                case TransformsWrapper.DETACHED:
                    return "DETACHED";
                default:
                    error.setError("TW002", "Unrecognized transformation");
                    return null;
            }
        }

        public static string getSignatureTypeTransform(TransformsWrapper transformsWrapper, Error error)
        {
            switch (transformsWrapper)
            {
                case TransformsWrapper.ENVELOPED:
                    return Constants.TRANSFORM_ENVELOPED_SIGNATURE;
                case TransformsWrapper.ENVELOPING:
                    return "http://www.w3.org/2000/09/xmldsig#enveloping-signature";
                case TransformsWrapper.DETACHED:
                    return "http://www.w3.org/2000/09/xmldsig#detached-signature";
                default:
                    error.setError("TW003", "Unrecognized transformation");
                    return null;

            }
        }

        public static string getCanonicalizationTransformation(CanonicalizerWrapper canonicalizerWrapper, Error error)
        {
            return CanonicalizerWrapperUtils.getCanonicalizationMethodAlorithm(canonicalizerWrapper, error);
        }

        /// <summary>
        /// Manage Enumerable enum 
        /// </summary>
        /// <typeparam name="TransformsWrapper">TransformsWrapper enum</typeparam>
        /// <returns>Enumerated values</returns>
        internal static IEnumerable<TransformsWrapper> GetValues<TransformsWrapper>()
        {
            return Enum.GetValues(typeof(TransformsWrapper)).Cast<TransformsWrapper>();
        }
    }

}
