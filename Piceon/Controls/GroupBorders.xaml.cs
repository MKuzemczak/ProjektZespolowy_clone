using System;
using System.Collections.Generic;
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

using Piceon.Helpers;
using Piceon.Models;

namespace Piceon.Controls
{
    public sealed partial class GroupBorders : UserControl
    {
        private GroupPosition _positionInGroup;

        public GroupPosition PositionInGroup
        {
            get { return (GroupPosition)GetValue(PositionInGroupProperty); }
            set
            {
                SetValue(PositionInGroupProperty, value);
                _positionInGroup = value;
                UpdateRectsVisibility();
            }
        }

        // Using a DependencyProperty as the backing store for PositionInGroup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionInGroupProperty =
            DependencyProperty.Register("PositionInGroup", typeof(GroupPosition), typeof(GroupBorders), new PropertyMetadata(0));


        public GroupBorders()
        {
            PositionInGroup = GroupPosition.None;
            this.InitializeComponent();
        }

        private void UpdateRectsVisibility()
        {
            CollapseAllRects();
            if (_positionInGroup == GroupPosition.Start)
            {
                leftRect.Visibility = Visibility.Visible;
            }
            if (_positionInGroup != GroupPosition.None)
            {
                topRect.Visibility = Visibility.Visible;
                bottomRect.Visibility = Visibility.Visible;
            }
            if (_positionInGroup == GroupPosition.End)
            {
                rightRect.Visibility = Visibility.Visible;
            }
        }

        private void CollapseAllRects()
        {
            if (leftRect is null || rightRect is null ||
                topRect is null || bottomRect is null)
                return;

            leftRect.Visibility = Visibility.Collapsed;
            rightRect.Visibility = Visibility.Collapsed;
            topRect.Visibility = Visibility.Collapsed;
            bottomRect.Visibility = Visibility.Collapsed;
        }
    }
}
