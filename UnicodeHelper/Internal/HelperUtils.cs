using System;
using System.Collections.Generic;

namespace UnicodeHelper.Internal
{
    internal static class HelperUtils
    {
        public static TextDirection DetermineDirection(IEnumerable<UCodepoint> codepoints)
        {
            if (codepoints == null)
                throw new ArgumentNullException(nameof(codepoints));

            bool inIsolate = false;
            foreach (UCodepoint uc in codepoints)
            {
                switch (UCodepoint.GetBidiClass(uc))
                {
                    case UnicodeBidiClass.LeftToRightIsolate: 
                    case UnicodeBidiClass.RightToLeftIsolate:
                        inIsolate = true;
                        break;
                    
                    case UnicodeBidiClass.PopDirectionalIsolate: 
                        inIsolate = false; 
                        break;

                    case UnicodeBidiClass.RightToLeft:
                    case UnicodeBidiClass.ArabicLetter:
                        if (!inIsolate)
                            return TextDirection.RtL;
                        break;
                    
                    case UnicodeBidiClass.LeftToRight:
                        if (!inIsolate)
                            return TextDirection.LtR;
                        break;
                }
            }
            
            return TextDirection.Undefined;
        }
    }
}
