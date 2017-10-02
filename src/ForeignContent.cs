#region Copyright (c) 2017 Atif Aziz, Adrian Guerra
//
// Portions Copyright (c) 2013 Ivan Nikulin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
#endregion

namespace ParseFive
{
    using System;
    using System.Collections.Generic;
    using Extensions;
    using static Truthiness;
    using T = HTML.TAG_NAMES;
    using NS = HTML.NAMESPACES;
    using ATTRS = HTML.ATTRS;

    static class ForeignContent
    {
        // MIME types

        static class MimeTypes
        {
            public const string TextHtml = "text/html";
            public const string ApplicationXml = "application/xhtml+xml";
        }

        // Attributes

        const string DefinitionUrlAttr = "definitionurl";
        const string AdjustedDefinitionUrlAttr = "definitionURL";

        static readonly IDictionary<string, string> SvgAttrsAdjustmentMap = new Dictionary<string, string>
        {
            { "attributename"      , "attributeName"       },
            { "attributetype"      , "attributeType"       },
            { "basefrequency"      , "baseFrequency"       },
            { "baseprofile"        , "baseProfile"         },
            { "calcmode"           , "calcMode"            },
            { "clippathunits"      , "clipPathUnits"       },
            { "diffuseconstant"    , "diffuseConstant"     },
            { "edgemode"           , "edgeMode"            },
            { "filterunits"        , "filterUnits"         },
            { "glyphref"           , "glyphRef"            },
            { "gradienttransform"  , "gradientTransform"   },
            { "gradientunits"      , "gradientUnits"       },
            { "kernelmatrix"       , "kernelMatrix"        },
            { "kernelunitlength"   , "kernelUnitLength"    },
            { "keypoints"          , "keyPoints"           },
            { "keysplines"         , "keySplines"          },
            { "keytimes"           , "keyTimes"            },
            { "lengthadjust"       , "lengthAdjust"        },
            { "limitingconeangle"  , "limitingConeAngle"   },
            { "markerheight"       , "markerHeight"        },
            { "markerunits"        , "markerUnits"         },
            { "markerwidth"        , "markerWidth"         },
            { "maskcontentunits"   , "maskContentUnits"    },
            { "maskunits"          , "maskUnits"           },
            { "numoctaves"         , "numOctaves"          },
            { "pathlength"         , "pathLength"          },
            { "patterncontentunits", "patternContentUnits" },
            { "patterntransform"   , "patternTransform"    },
            { "patternunits"       , "patternUnits"        },
            { "pointsatx"          , "pointsAtX"           },
            { "pointsaty"          , "pointsAtY"           },
            { "pointsatz"          , "pointsAtZ"           },
            { "preservealpha"      , "preserveAlpha"       },
            { "preserveaspectratio", "preserveAspectRatio" },
            { "primitiveunits"     , "primitiveUnits"      },
            { "refx"               , "refX"                },
            { "refy"               , "refY"                },
            { "repeatcount"        , "repeatCount"         },
            { "repeatdur"          , "repeatDur"           },
            { "requiredextensions" , "requiredExtensions"  },
            { "requiredfeatures"   , "requiredFeatures"    },
            { "specularconstant"   , "specularConstant"    },
            { "specularexponent"   , "specularExponent"    },
            { "spreadmethod"       , "spreadMethod"        },
            { "startoffset"        , "startOffset"         },
            { "stddeviation"       , "stdDeviation"        },
            { "stitchtiles"        , "stitchTiles"         },
            { "surfacescale"       , "surfaceScale"        },
            { "systemlanguage"     , "systemLanguage"      },
            { "tablevalues"        , "tableValues"         },
            { "targetx"            , "targetX"             },
            { "targety"            , "targetY"             },
            { "textlength"         , "textLength"          },
            { "viewbox"            , "viewBox"             },
            { "viewtarget"         , "viewTarget"          },
            { "xchannelselector"   , "xChannelSelector"    },
            { "ychannelselector"   , "yChannelSelector"    },
            { "zoomandpan"         , "zoomAndPan"          },
        };

        sealed class XmlAdjustment
        {
            public readonly string Prefix;
            public readonly string Name;
            public readonly string Namespace;

            public XmlAdjustment(string prefix, string name, string @namespace)
            {
                Prefix = prefix;
                Name = name;
                Namespace = @namespace;
            }
        }

        static readonly IDictionary<string, XmlAdjustment> XmlAttrsAdjustmentMap = new Dictionary<string, XmlAdjustment>
        {
            ["xlink:actuate"] = new XmlAdjustment("xlink", "actuate", NS.XLINK),
            ["xlink:arcrole"] = new XmlAdjustment("xlink", "arcrole", NS.XLINK),
            ["xlink:href"   ] = new XmlAdjustment("xlink", "href"   , NS.XLINK),
            ["xlink:role"   ] = new XmlAdjustment("xlink", "role"   , NS.XLINK),
            ["xlink:show"   ] = new XmlAdjustment("xlink", "show"   , NS.XLINK),
            ["xlink:title"  ] = new XmlAdjustment("xlink", "title"  , NS.XLINK),
            ["xlink:type"   ] = new XmlAdjustment("xlink", "type"   , NS.XLINK),
            ["xml:base"     ] = new XmlAdjustment("xml"  , "base"   , NS.XML),
            ["xml:lang"     ] = new XmlAdjustment("xml"  , "lang"   , NS.XML),
            ["xml:space"    ] = new XmlAdjustment("xml"  , "space"  , NS.XML),
            ["xmlns"        ] = new XmlAdjustment(""     , "xmlns"  , NS.XMLNS),
            ["xmlns:xlink"  ] = new XmlAdjustment("xmlns", "xlink"  , NS.XMLNS)
        };

        // SVG tag names adjustment map

        static readonly IDictionary<string, string> SvgTagNamesAdjustmentMap = new Dictionary<string, string>
        {
            ["altglyph"]            = "altGlyph",
            ["altglyphdef"]         = "altGlyphDef",
            ["altglyphitem"]        = "altGlyphItem",
            ["animatecolor"]        = "animateColor",
            ["animatemotion"]       = "animateMotion",
            ["animatetransform"]    = "animateTransform",
            ["clippath"]            = "clipPath",
            ["feblend"]             = "feBlend",
            ["fecolormatrix"]       = "feColorMatrix",
            ["fecomponenttransfer"] = "feComponentTransfer",
            ["fecomposite"]         = "feComposite",
            ["feconvolvematrix"]    = "feConvolveMatrix",
            ["fediffuselighting"]   = "feDiffuseLighting",
            ["fedisplacementmap"]   = "feDisplacementMap",
            ["fedistantlight"]      = "feDistantLight",
            ["feflood"]             = "feFlood",
            ["fefunca"]             = "feFuncA",
            ["fefuncb"]             = "feFuncB",
            ["fefuncg"]             = "feFuncG",
            ["fefuncr"]             = "feFuncR",
            ["fegaussianblur"]      = "feGaussianBlur",
            ["feimage"]             = "feImage",
            ["femerge"]             = "feMerge",
            ["femergenode"]         = "feMergeNode",
            ["femorphology"]        = "feMorphology",
            ["feoffset"]            = "feOffset",
            ["fepointlight"]        = "fePointLight",
            ["fespecularlighting"]  = "feSpecularLighting",
            ["fespotlight"]         = "feSpotLight",
            ["fetile"]              = "feTile",
            ["feturbulence"]        = "feTurbulence",
            ["foreignobject"]       = "foreignObject",
            ["glyphref"]            = "glyphRef",
            ["lineargradient"]      = "linearGradient",
            ["radialgradient"]      = "radialGradient",
            ["textpath"]            = "textPath",
        };

        //Tags that causes exit from foreign content

        static readonly IDictionary<string, bool> ExitsForeignContent = new Dictionary<string, bool>
        {
            [T.B         ] = true,
            [T.BIG       ] = true,
            [T.BLOCKQUOTE] = true,
            [T.BODY      ] = true,
            [T.BR        ] = true,
            [T.CENTER    ] = true,
            [T.CODE      ] = true,
            [T.DD        ] = true,
            [T.DIV       ] = true,
            [T.DL        ] = true,
            [T.DT        ] = true,
            [T.EM        ] = true,
            [T.EMBED     ] = true,
            [T.H1        ] = true,
            [T.H2        ] = true,
            [T.H3        ] = true,
            [T.H4        ] = true,
            [T.H5        ] = true,
            [T.H6        ] = true,
            [T.HEAD      ] = true,
            [T.HR        ] = true,
            [T.I         ] = true,
            [T.IMG       ] = true,
            [T.LI        ] = true,
            [T.LISTING   ] = true,
            [T.MENU      ] = true,
            [T.META      ] = true,
            [T.NOBR      ] = true,
            [T.OL        ] = true,
            [T.P         ] = true,
            [T.PRE       ] = true,
            [T.RUBY      ] = true,
            [T.S         ] = true,
            [T.SMALL     ] = true,
            [T.SPAN      ] = true,
            [T.STRONG    ] = true,
            [T.STRIKE    ] = true,
            [T.SUB       ] = true,
            [T.SUP       ] = true,
            [T.TABLE     ] = true,
            [T.TT        ] = true,
            [T.U         ] = true,
            [T.UL        ] = true,
            [T.VAR       ] = true,
        };

        //Check exit from foreign content

        public static bool CausesExit(StartTagToken startTagToken)
        {
            var tn = startTagToken.TagName;
            var isFontWithAttrs = tn == T.FONT && (Tokenizer.GetTokenAttr(startTagToken, ATTRS.COLOR) != null ||
                                                    Tokenizer.GetTokenAttr(startTagToken, ATTRS.SIZE) != null ||
                                                    Tokenizer.GetTokenAttr(startTagToken, ATTRS.FACE) != null);

            return isFontWithAttrs ? true : ExitsForeignContent.TryGetValue(tn, out var value) ? value : false;
        }

        //Token adjustments

        public static void AdjustTokenMathMlAttrs(StartTagToken token)
        {
            foreach (var attr in token.Attrs)
            {
                if (attr.Name == DefinitionUrlAttr)
                {
                    attr.Name = AdjustedDefinitionUrlAttr;
                    break;
                }
            }
        }

        public static void AdjustTokenSvgAttrs(StartTagToken token)
        {
            foreach (var attr in token.Attrs)
            {
                if (SvgAttrsAdjustmentMap.TryGetValue(attr.Name, out var adjustedAttrName))
                    attr.Name = adjustedAttrName;
            }
        }

        public static void AdjustTokenXmlAttrs(StartTagToken token)
        {
            foreach (var attr in token.Attrs)
            {
                if (XmlAttrsAdjustmentMap.TryGetValue(attr.Name, out var adjustedAttrEntry))
                {
                    attr.Prefix = adjustedAttrEntry.Prefix;
                    attr.Name = adjustedAttrEntry.Name;
                    attr.Namespace = adjustedAttrEntry.Namespace;
                }
            }
        }

        public static void AdjustTokenSvgTagName(StartTagToken token)
        {
            if (SvgTagNamesAdjustmentMap.TryGetValue(token.TagName, out var adjustedTagName))
                token.TagName = adjustedTagName;
        }

        //Integration points

        static bool IsMathMlTextIntegrationPoint(string tn, string ns)
        {
            return ns == NS.MATHML && (tn == T.MI || tn == T.MO || tn == T.MN || tn == T.MS || tn == T.MTEXT);
        }

        static bool IsHtmlIntegrationPoint(string tn, string ns, ArraySegment<(string Name, string Value)> attrs)
        {
            if (ns == NS.MATHML && tn == T.ANNOTATION_XML)
            {
                foreach (var attr in attrs)
                {
                    if (attr.Name == ATTRS.ENCODING)
                    {
                        var value = attr.Value.ToLowerCase();

                        return value == MimeTypes.TextHtml || value == MimeTypes.ApplicationXml;
                    }
                }
            }

            return ns == NS.SVG && (tn == T.FOREIGN_OBJECT || tn == T.DESC || tn == T.TITLE);
        }

        public static bool IsIntegrationPoint(string tn, string ns, ArraySegment<(string, string)> attrs, string foreignNS)
        {
            if ((!IsTruthy(foreignNS) || foreignNS == NS.HTML) && IsHtmlIntegrationPoint(tn, ns, attrs))
                return true;

            if ((!IsTruthy(foreignNS) || foreignNS == NS.MATHML) && IsMathMlTextIntegrationPoint(tn, ns))
                return true;

            return false;
        }
    }
}