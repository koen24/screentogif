﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace ScreenToGif.Controls
{
    /// <summary>
    /// Basic class of a Hideable TabControl.
    /// </summary>
    public class HideableTabControl : TabControl
    {
        #region Variables

        private Button _button;
        private TabPanel _tabPanel;
        private Border _border;

        #endregion

        static HideableTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HideableTabControl), new FrameworkPropertyMetadata(typeof(HideableTabControl)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _button = Template.FindName("HideGridButton", this) as Button;
            _tabPanel = Template.FindName("TabPanel", this) as TabPanel;
            _border = Template.FindName("ContentBorder", this) as Border;
            

            if (_button != null)
                _button.PreviewMouseUp += Button_Clicked;

            if (_tabPanel != null)
            {
                foreach (TabItem tabItem in _tabPanel.Children)
                {
                    tabItem.PreviewMouseDown += TabItem_PreviewMouseDown;
                }

                _tabPanel.PreviewMouseWheel += TabControl_PreviewMouseWheel;
            }
        }

        #region Events

        private void TabControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                if (SelectedIndex < Items.Count - 1)
                    SelectedIndex++;
                else
                    SelectedIndex = 0;
            }
            else
            {

                if (SelectedIndex > 0)
                    SelectedIndex--;
                else
                    SelectedIndex = Items.Count - 1;
            }

            ChangeVisibility();
        }

        private void TabItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var selected = sender as TabItem;

            if (selected != null)
                selected.IsSelected = true;

            if (Math.Abs(_border.ActualHeight - 100) < 0)
                return;

            var animation = new DoubleAnimation(_border.ActualHeight, 100, new Duration(new TimeSpan(0, 0, 0, 1)));
            animation.EasingFunction = new PowerEase() { Power = 8 };
            _border.BeginAnimation(HeightProperty, animation);

            var opacityAnimation = new DoubleAnimation(_border.Opacity, 1, new Duration(new TimeSpan(0, 0, 0, 1)));
            opacityAnimation.EasingFunction = new PowerEase() { Power = 8 };
            _border.BeginAnimation(OpacityProperty, opacityAnimation);

            var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
            _button.BeginAnimation(VisibilityProperty, visibilityAnimation);
        }

        private void Button_Clicked(object sender, MouseButtonEventArgs e)
        {
            var animation = new DoubleAnimation(_border.ActualHeight, 0, new Duration(new TimeSpan(0, 0, 0, 1)));
            animation.EasingFunction = new PowerEase() { Power = 8};
            _border.BeginAnimation(HeightProperty, animation);

            var opacityAnimation = new DoubleAnimation(_border.Opacity, 0, new Duration(new TimeSpan(0, 0, 0, 1)));
            opacityAnimation.EasingFunction = new PowerEase() { Power = 8 };
            _border.BeginAnimation(OpacityProperty, opacityAnimation);

            var objectAnimation = new ObjectAnimationUsingKeyFrames();
            objectAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame(null, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
            this.BeginAnimation(SelectedItemProperty, objectAnimation);

            var visibilityAnimation = new ObjectAnimationUsingKeyFrames();
            visibilityAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Hidden, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(0.5))));
            _button.BeginAnimation(VisibilityProperty, visibilityAnimation);

            //var selectedAnimation = new BooleanAnimationUsingKeyFrames();
            //selectedAnimation.KeyFrames.Add(new DiscreteBooleanKeyFrame(false, KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 1))));
            //_border.BeginAnimation(SelectedItemProperty, selectedAnimation);

            #region Old Code

            //this.SelectedItem = null;

            //foreach (TabItem child in _tabPanel.Children)
            //{
            //    child.IsSelected = false;
            //}

            //_border.Visibility = Visibility.Collapsed;
            //_button.Visibility = Visibility.Collapsed;

            #endregion
        }

        #endregion

        /// <summary>
        /// Changes the visibility of the Content.
        /// </summary>
        /// <param name="visible">True to show the Content.</param>
        public void ChangeVisibility(bool visible = true)
        {
            _border.Visibility = visible? Visibility.Visible: Visibility.Collapsed;
            _button.Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
