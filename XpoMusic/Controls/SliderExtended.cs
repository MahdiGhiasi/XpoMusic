using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace XpoMusic.Controls
{
    public sealed class SliderExtended : Slider
    {
        public event EventHandler<SliderExtendedValueChangedEventArgs> ValueChangedByUserManipulation;

        private Thumb horizontalThumb, verticalThumb;
        private double valueToSetWithoutNotifying;
        private DateTime value2lastSetTime;
        private bool isBeingDragged;
        private double draggedInitialValue;

        public SliderExtended()
        {
            this.DefaultStyleKey = typeof(SliderExtended);

            isBeingDragged = false;
        }

        /// <summary>
        /// If this is true, ValueChangedByUserManipulation will be raised in real time while
        /// user is dragging the thumb, and also Value2 will be updated in real time.
        /// But if this is false, ValueChangedByUserManipulation will be raised and Value2 will
        /// be updated only on the beginning of dragging and after user finishes dragging the thumb.
        /// 
        /// NOTE: Disabling this will cause a slight lag in raising ValueChanged and 
        /// ValueChangedByUserManipulation events, and also updating Value2 when user
        /// changes the value.
        /// </summary>
        public bool ContinousUpdateWhileDragging
        {
            get { return (bool)GetValue(ContinousUpdateWhileDraggingProperty); }
            set { SetValue(ContinousUpdateWhileDraggingProperty, value); }
        }

        public static readonly DependencyProperty ContinousUpdateWhileDraggingProperty =
            DependencyProperty.Register("ContinousUpdateWhileDragging", typeof(bool), typeof(SliderExtended), new PropertyMetadata(true));

        /// <summary>
        /// Values set using this property does not cause ValueChanged event to raise, and also 
        /// will be ignored if user is dragging the slider at the same time.
        /// If ContinousUpdateWhileDragging is false, when user drags the thumb, this value will only
        /// be updated on the beginning and the end of dragging. But if it's true, this value will be 
        /// updated in real time.
        /// </summary>
        public double Value2
        {
            get => (double)GetValue(Value2Property);
            set
            {
                if (isBeingDragged)
                    return;

                SetValue(Value2Property, value);
                if (Value != value)
                {
                    valueToSetWithoutNotifying = value;
                    value2lastSetTime = DateTime.UtcNow;
                    Value = value;
                }
            }
        }
        public static readonly DependencyProperty Value2Property =
            DependencyProperty.Register("Value2", typeof(double), typeof(SliderExtended), new PropertyMetadata(0));

        public double KeyboardStepFrequency
        {
            get { return (double)GetValue(KeyboardStepFrequencyProperty); }
            set { SetValue(KeyboardStepFrequencyProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyboardStepFrequency.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyboardStepFrequencyProperty =
            DependencyProperty.Register("KeyboardStepFrequency", typeof(double), typeof(SliderExtended), new PropertyMetadata(1));

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            horizontalThumb = GetTemplateChild("HorizontalThumb") as Thumb;
            verticalThumb = GetTemplateChild("VerticalThumb") as Thumb;

            horizontalThumb.DragStarted += Thumb_DragStarted;
            horizontalThumb.DragCompleted += Thumb_DragCompleted;
            verticalThumb.DragStarted += Thumb_DragStarted;
            verticalThumb.DragCompleted += Thumb_DragCompleted;

            this.ManipulationMode = ManipulationModes.TranslateRailsX;
            this.ManipulationStarted += SliderExtended_ManipulationStarted;
            this.ManipulationCompleted += SliderExtended_ManipulationCompleted;
        }

        private void SliderExtended_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            isBeingDragged = false;

            if (!ContinousUpdateWhileDragging)
                OnValueChanged(draggedInitialValue, Value);
        }

        private void SliderExtended_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            isBeingDragged = true;
            draggedInitialValue = Value2;
        }

        private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
        {
            isBeingDragged = true;
            draggedInitialValue = Value2;
        }

        private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            isBeingDragged = false;

            if (!ContinousUpdateWhileDragging)
                OnValueChanged(draggedInitialValue, Value);
        }

        protected override async void OnValueChanged(double oldValue, double newValue)
        {
            base.OnValueChanged(oldValue, newValue);

            if (newValue == valueToSetWithoutNotifying 
                && DateTime.UtcNow - value2lastSetTime < TimeSpan.FromMilliseconds(10))
            {
                // Don't fire ValueChangedByUserManipulation as this change is done via Value2.
                return;
            }

            if (!ContinousUpdateWhileDragging)
                await Task.Delay(10);

            if (!ContinousUpdateWhileDragging && isBeingDragged)
                return;

            ValueChangedByUserManipulation?.Invoke(this, new SliderExtendedValueChangedEventArgs
            {
                OldValue = oldValue,
                NewValue = newValue,
            });

            // Also set the Value2 DependencyProperty directly
            SetValue(Value2Property, newValue);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Right || e.Key == Windows.System.VirtualKey.Up)
                Value = Math.Min(Value + KeyboardStepFrequency, Maximum);
            else if (e.Key == Windows.System.VirtualKey.Left || e.Key == Windows.System.VirtualKey.Down)
                Value = Math.Max(Value - KeyboardStepFrequency, Minimum);
            else
                base.OnKeyDown(e);
        }
    }

    public class SliderExtendedValueChangedEventArgs
    {
        public double OldValue { get; set; }
        public double NewValue { get; set; }
    }
}
