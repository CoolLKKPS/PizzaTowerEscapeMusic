using Newtonsoft.Json;
using System;
using System.Reflection;

namespace PizzaTowerEscapeMusic.Scripting.Conditions
{
    public class Condition_Weather : Condition
    {
        public override bool Check(Script script)
        {
            if (TimeOfDay.Instance == null)
            {
                return false;
            }
            LevelWeatherType currentLevelWeather = TimeOfDay.Instance.currentLevelWeather;
            object obj = this.weather;
            if (obj is LevelWeatherType)
            {
                LevelWeatherType levelWeatherType = (LevelWeatherType)obj;
                return currentLevelWeather == levelWeatherType;
            }
            string text = this.weather as string;
            if (text == null || Condition_Weather._weatherConfigHelperType == null || Condition_Weather._resolveWeatherMethod == null || Condition_Weather._vanillaWeatherTypeProp == null)
            {
                return false;
            }
            bool flag;
            try
            {
                object obj2 = Condition_Weather._resolveWeatherMethod.Invoke(null, new object[] { text });
                if (obj2 == null)
                {
                    flag = false;
                }
                else
                {
                    LevelWeatherType levelWeatherType2 = (LevelWeatherType)Condition_Weather._vanillaWeatherTypeProp.GetValue(obj2);
                    flag = currentLevelWeather == levelWeatherType2;
                }
            }
            catch (Exception)
            {
                flag = false;
            }
            return flag;
        }

        static Condition_Weather()
        {
            try
            {
                Condition_Weather._weatherConfigHelperType = Type.GetType("WeatherRegistry.ConfigHelper, WeatherRegistry");
                if (Condition_Weather._weatherConfigHelperType != null)
                {
                    Condition_Weather._resolveWeatherMethod = Condition_Weather._weatherConfigHelperType.GetMethod("ResolveStringToWeather", new Type[] { typeof(string) });
                    Type type = Type.GetType("WeatherRegistry.Weather, WeatherRegistry");
                    if (type != null)
                    {
                        Condition_Weather._vanillaWeatherTypeProp = type.GetProperty("VanillaWeatherType");
                    }
                }
            }
            catch (Exception)
            {
                Condition_Weather._weatherConfigHelperType = null;
                Condition_Weather._resolveWeatherMethod = null;
                Condition_Weather._vanillaWeatherTypeProp = null;
            }
        }

        [JsonRequired]
        public object weather;

        private static Type _weatherConfigHelperType;

        private static MethodInfo _resolveWeatherMethod;

        private static PropertyInfo _vanillaWeatherTypeProp;
    }
}
