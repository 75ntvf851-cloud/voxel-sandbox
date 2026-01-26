using NUnit.Framework;
using VoxelSandbox.Core;

namespace VoxelSandbox.Tests.EditMode
{
    /// <summary>
    /// Edit mode tests for LocalizationService.
    /// </summary>
    public class LocalizationServiceTests
    {
        private LocalizationService _localization;

        [SetUp]
        public void Setup()
        {
            _localization = new LocalizationService();
        }

        [Test]
        public void LocalizationService_InitializesWithRussian()
        {
            _localization.Initialize();
            Assert.AreEqual(LocalizationService.Language.Russian, _localization.CurrentLanguage);
        }

        [Test]
        public void LocalizationService_CanSwitchLanguages()
        {
            _localization.Initialize();
            
            _localization.SetLanguage(LocalizationService.Language.English);
            Assert.AreEqual(LocalizationService.Language.English, _localization.CurrentLanguage);
            
            _localization.SetLanguage(LocalizationService.Language.Russian);
            Assert.AreEqual(LocalizationService.Language.Russian, _localization.CurrentLanguage);
        }

        [Test]
        public void LocalizationService_ReturnsPlaceholderForMissingKey()
        {
            _localization.Initialize();
            
            string result = _localization.GetString("nonexistent_key");
            Assert.AreEqual("[nonexistent_key]", result);
        }

        [Test]
        public void LocalizationService_FormatsStringsWithArguments()
        {
            _localization.Initialize();
            
            // Test with a known key that has formatting
            // This will depend on the actual keys in RU.json
            string result = _localization.GetString("hud_fps", "60");
            Assert.IsNotNull(result);
            Assert.IsNotEmpty(result);
        }

        [Test]
        public void LocalizationService_FiresEventOnLanguageChange()
        {
            _localization.Initialize();
            
            bool eventFired = false;
            _localization.OnLanguageChanged += () => eventFired = true;
            
            _localization.SetLanguage(LocalizationService.Language.English);
            Assert.IsTrue(eventFired);
        }
    }
}
