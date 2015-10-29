using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using App1.Models;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace App1
{
    public sealed partial class LocationPin : UserControl
    {
        private string imagePath;
        private BaseLocation _details;

        public BaseLocation Details
        {
            get { return _details; }
            set
            {
                if (_details == value) return;
                _details = value;
                textName.Text = _details.Name;
            }
        }

        public string ImagePath
        {

            set
            {
                if (value!=imagePath)
                {
                    imagePath = value;
                    BitmapImage imgeSource =new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute));
                    iconImage.Source = imgeSource;
                }

            }
        }
        public LocationPin()
        {
            this.InitializeComponent();
        }
        private void iconImage_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ((Frame) Window.Current.Content).Navigate(typeof (DetailsPage), _details);
        }
    }
}
