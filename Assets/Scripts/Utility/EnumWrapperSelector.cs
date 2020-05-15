using System;
using Networking;

namespace Utility
{
    public enum EnumWrapperEnum
    {
        Map,
        GameMode
    }
    
    public static class EnumWrapperSelector
    {

        public static EnumWrapper FromEnum(EnumWrapperEnum e)
        {

            switch (e)
            {
                
                case EnumWrapperEnum.Map:
                    
                    return new MapEnumWrapper();
                
                case EnumWrapperEnum.GameMode:
                    
                    return new GameModeEnumWrapper();
                
                default:
                    
                    throw new ArgumentException(e + " is not a valid EnumWrapperEnum!");
                
            }

        }
    
    }
}