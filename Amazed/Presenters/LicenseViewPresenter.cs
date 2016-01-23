using System;
using DreamAmazon.Interfaces;
using DreamAmazon.Models;
using Microsoft.Practices.ServiceLocation;

namespace DreamAmazon.Presenters
{
    public class LicenseViewPresenter : BasePresenter
    {
        private readonly ILicenseView _view;

        public bool RealClose { get; protected set; }

        public LicenseModel License;
        private readonly SettingModel _setting;
        private readonly ILogger _logger;

        public LicenseViewPresenter(ILicenseView view)
        {
            Contracts.Require(view != null);

            var settingsService = ServiceLocator.Current.GetInstance<ISettingsService>();
            _setting = settingsService.GetSettings();
            _logger = ServiceLocator.Current.GetInstance<ILogger>();

            _view = view;
            License = new LicenseModel();
            License.PropertyChanged += License_PropertyChanged;

            _view.ValidateLicense += View_ValidateLicense;

            RealClose = true;
        }

        private async void View_ValidateLicense(object sender, string e)
        {
            _setting.LicenseKey = License.LicenseKey;

            _view.DisableFileds();

            bool initResult;

            try
            {
                initResult = await DreamAmazon.License.InitAsync(_setting.LicenseKey);
            }
            catch (Exception exception)
            {
                _logger.Error(exception);

                _view.ShowMessage("Error while initializing components !", MessageType.Error);

                RealClose = true;
                _view.EnableFields();
                return;
            }

            if (!initResult)
            {
                _view.ShowMessage("Your license key is invalid !", MessageType.Error);
                _setting.LicenseKey = string.Empty;
                RealClose = true;
            }
            else
            {
                RealClose = false;
            }

            _view.EnableFields();
        }

        private void License_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "LicenseKey")
            {
                _view.SetValidationEnable(License.LicenseKey.Length == 16);
            }
        }

        public void Start()
        {
            if (IsViewActive("frmLicense"))
            {
                _view.BindSettings(License);
                _view.Show();
            }
        }
    }
}