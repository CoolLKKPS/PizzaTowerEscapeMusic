using LethalConfig;
using LethalConfig.ConfigItems;
using LethalConfig.ConfigItems.Options;

namespace PizzaTowerEscapeMusic
{
    internal static class LethalConfigIntegration
    {
        internal static void Initialize()
        {
            LethalConfigManager.SkipAutoGen();

            LethalConfigManager.SetModDescription("PizzaTowerEscapeMusic");
            var masterVolumeEntry = PizzaTowerEscapeMusicManager.Configuration.volumeMaster;
            var slider = new FloatSliderConfigItem(masterVolumeEntry, new FloatStepSliderOptions
            {
                Name = "Master Volume",
                RequiresRestart = false,
                Min = 0f,
                Max = 1f,
                Step = 0.01f
            });
            LethalConfigManager.AddConfigItem(slider);

            var selectLabelEntry = PizzaTowerEscapeMusicManager.Configuration.selectLabelManually;
            var textInput = new TextInputFieldConfigItem(selectLabelEntry, new TextInputFieldOptions
            {
                Name = "Select Label Manually",
                RequiresRestart = false,
                CharacterLimit = 200
            });
            LethalConfigManager.AddConfigItem(textInput);
        }
    }
}