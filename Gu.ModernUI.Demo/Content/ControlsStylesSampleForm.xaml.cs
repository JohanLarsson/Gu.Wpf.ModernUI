﻿namespace Gu.ModernUI.Demo.Content
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Gu.ModernUI.Windows;
    using Gu.ModernUI.Windows.Controls;
    using Gu.ModernUI.Windows.Navigation;

    /// <summary>
    /// Interaction logic for ControlsStylesSampleForm.xaml
    /// </summary>
    public partial class ControlsStylesSampleForm : UserControl, IContent
    {
        private readonly SampleFormViewModel vm = new SampleFormViewModel();
        public ControlsStylesSampleForm()
        {
            InitializeComponent();
            this.DataContext = this.vm;
            this.Loaded += OnLoaded;
        }

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            // select first control on the form
            Keyboard.Focus(this.TextFirstName);
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e)
        {
        }

        public void OnNavigatedFrom(NavigationEventArgs e)
        {
        }

        public void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (this.vm.IsDirty)
            {
                ModernDialog.ShowMessage("The form is dirty. Submit changes before navigating away.", "Message Dialog", MessageBoxButton.OK);
            }
            e.Cancel = this.vm.IsDirty;
        }
    }
}
