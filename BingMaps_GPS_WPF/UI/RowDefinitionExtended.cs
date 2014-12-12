/// WPF Grid Column and Row Hiding - CodeProject
/// http://www.codeproject.com/Articles/437237/WPF-Grid-Column-and-Row-Hiding
/// 
/// Author:immortalus
/// immortalus - Professional Profile - CodeProject
/// http://www.codeproject.com/Members/immortalus
///
/// License:CPOL
/// CPOL: Code Project Open License - CodeProject
/// http://www.codeproject.com/info/cpol10.aspx
///
/// THIS WORK IS PROVIDED "AS IS", "WHERE IS" AND "AS AVAILABLE", WITHOUT ANY EXPRESS OR IMPLIED WARRANTIES OR CONDITIONS OR GUARANTEES. YOU, THE USER, ASSUME ALL RISK IN ITS USE, INCLUDING COPYRIGHT INFRINGEMENT, PATENT INFRINGEMENT, SUITABILITY, ETC. AUTHOR EXPRESSLY DISCLAIMS ALL EXPRESS, IMPLIED OR STATUTORY WARRANTIES OR CONDITIONS, INCLUDING WITHOUT LIMITATION, WARRANTIES OR CONDITIONS OF MERCHANTABILITY, MERCHANTABLE QUALITY OR FITNESS FOR A PARTICULAR PURPOSE, OR ANY WARRANTY OF TITLE OR NON-INFRINGEMENT, OR THAT THE WORK (OR ANY PORTION THEREOF) IS CORRECT, USEFUL, BUG-FREE OR FREE OF VIRUSES. YOU MUST PASS THIS DISCLAIMER ON WHENEVER YOU DISTRIBUTE THE WORK OR DERIVATIVE WORKS.

using System;
using System.Windows;
using System.Windows.Controls;

namespace GridTest.Extended
{
    public class RowDefinitionExtended : RowDefinition
    {
        // Variables
        public static DependencyProperty VisibleProperty;

        // Properties
        public Boolean Visible { get { return (Boolean)GetValue(VisibleProperty); } set { SetValue(VisibleProperty, value); } }

        // Constructors
        static RowDefinitionExtended()
        {
            VisibleProperty = DependencyProperty.Register("Visible", typeof(Boolean), typeof(RowDefinitionExtended), new PropertyMetadata(true, new PropertyChangedCallback(OnVisibleChanged)));
            RowDefinition.HeightProperty.OverrideMetadata(typeof(RowDefinitionExtended), new FrameworkPropertyMetadata(new GridLength(1, GridUnitType.Star), null, new CoerceValueCallback(CoerceHeight)));
            RowDefinition.MinHeightProperty.OverrideMetadata(typeof(RowDefinitionExtended), new FrameworkPropertyMetadata((Double)0, null, new CoerceValueCallback(CoerceMinHeight)));
        }

        // Get/Set
        public static void SetVisible(DependencyObject obj, Boolean nVisible)
        {
            obj.SetValue(VisibleProperty, nVisible);
        }
        public static Boolean GetVisible(DependencyObject obj)
        {
            return (Boolean)obj.GetValue(VisibleProperty);
        }

        static void OnVisibleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            obj.CoerceValue(RowDefinition.HeightProperty);
            obj.CoerceValue(RowDefinition.MinHeightProperty);
        }
        static Object CoerceHeight(DependencyObject obj, Object nValue)
        {
            return (((RowDefinitionExtended)obj).Visible) ? nValue : new GridLength(0);
        }
        static Object CoerceMinHeight(DependencyObject obj, Object nValue)
        {
            return (((RowDefinitionExtended)obj).Visible) ? nValue : (Double)0;
        }
    }
}
