﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;

namespace TestApplication.Tests
{
    public partial class TemplatedTextBoxTest : Page
    {
        public TemplatedTextBoxTest()
        {
            InitializeComponent();
        }

        // Executes when the user navigates to this page.
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        void ButtonTestNormalTextBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAllTestNormalTextBox.SelectAll();
        }

        void ButtonTestTemplatedTextBoxSelectAll_Click(object sender, RoutedEventArgs e)
        {
            SelectAllTestTemplatedTextBox.SelectAll();
        }
    }
}
