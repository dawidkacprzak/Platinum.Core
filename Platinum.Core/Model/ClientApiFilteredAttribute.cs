using System;
using Platinum.Core.Types;

namespace Platinum.Core.Model
{
    public class ClientApiFilteredAttribute
    {
        public string AttributeName { get; set; }
        public string AttributeValue { get; set; }
        public EAttributeCompareType AttributeCompareType { get; set; }

        public void Validate()
        {
            if (AttributeCompareType == EAttributeCompareType.NoInfo)
            {
                throw new Exception("AttributeCompareType must be integer in range 1-4");
            }
            else
            {
                if (AttributeCompareType == EAttributeCompareType.Bigger
                    || AttributeCompareType == EAttributeCompareType.Less)
                {
                    bool parsable = float.TryParse(AttributeValue, out _);
                    if (!parsable)
                    {
                        bool parsable2 = int.TryParse(AttributeValue, out _);
                        if (!parsable2)
                        {
                            throw new Exception("Attribute value cannot be parsed to numeric value. Numeric value is required if AttributeCompareType is bigger (2) or less (3)");
                        }
                    }
                }
            }
        }
    }
}