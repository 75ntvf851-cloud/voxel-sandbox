using NUnit.Framework;
using VoxelSandbox.Core;

namespace VoxelSandbox.Tests.EditMode
{
    /// <summary>
    /// Edit mode tests for SettingsService.
    /// </summary>
    public class SettingsServiceTests
    {
        private SettingsService _settings;

        [SetUp]
        public void Setup()
        {
            _settings = new SettingsService();
            _settings.Initialize();
        }

        [Test]
        public void SettingsService_InitializesWithDefaults()
        {
            Assert.IsNotNull(_settings.Settings);
            Assert.AreEqual(LocalizationService.Language.Russian, _settings.Settings.language);
            Assert.AreEqual(2.0f, _settings.Settings.mouseSensitivity);
            Assert.AreEqual(false, _settings.Settings.invertY);
            Assert.AreEqual(1.0f, _settings.Settings.masterVolume);
        }

        [Test]
        public void SettingsService_CanSetLanguage()
        {
            _settings.SetLanguage(LocalizationService.Language.English);
            Assert.AreEqual(LocalizationService.Language.English, _settings.Settings.language);
        }

        [Test]
        public void SettingsService_ClampsMouseSensitivity()
        {
            _settings.SetMouseSensitivity(15f); // Above max
            Assert.LessOrEqual(_settings.Settings.mouseSensitivity, 10f);
            
            _settings.SetMouseSensitivity(0.05f); // Below min
            Assert.GreaterOrEqual(_settings.Settings.mouseSensitivity, 0.1f);
        }

        [Test]
        public void SettingsService_ClampsVolume()
        {
            _settings.SetMasterVolume(2f); // Above max
            Assert.LessOrEqual(_settings.Settings.masterVolume, 1f);
            
            _settings.SetMasterVolume(-1f); // Below min
            Assert.GreaterOrEqual(_settings.Settings.masterVolume, 0f);
        }

        [Test]
        public void SettingsService_FiresEventOnChange()
        {
            bool eventFired = false;
            _settings.OnSettingsChanged += () => eventFired = true;
            
            _settings.SetLanguage(LocalizationService.Language.English);
            Assert.IsTrue(eventFired);
        }

        [Test]
        public void SettingsService_CanResetToDefaults()
        {
            _settings.SetMouseSensitivity(5f);
            _settings.SetInvertY(true);
            
            _settings.ResetToDefaults();
            
            Assert.AreEqual(2.0f, _settings.Settings.mouseSensitivity);
            Assert.AreEqual(false, _settings.Settings.invertY);
        }
    }
}
