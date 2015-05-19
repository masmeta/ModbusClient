using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Net;
//using Microsoft.Windows.Themes;
using System.Text.RegularExpressions;
using System.Collections;

namespace IPScan.Controls
{
    public enum IPClass
    {
        ClassA,
        ClassB,
        ClassC,
        Undefined
    }

    public delegate void ClassChangedEventHandler(object sender, IPClass NewClass);

    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class IPAddressTextBox : UserControl, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        #endregion

        #region ClassChangedEvent

        public event ClassChangedEventHandler ClassChanged;

        private void NotifyClassChanged(IPClass newClass)
        {
            if (ClassChanged != null)
            {
                ClassChanged(this, newClass);
            }
        }

        #endregion

        #region Class Variables

        private byte[] m_IPAddress =  { 0, 0, 0, 0 };

        private IPClass m_AddressClass = IPClass.Undefined;

        #endregion

        #region Constructor

        public IPAddressTextBox()
        {
            InitializeComponent();

           // this.DataContext = this;
        }

        #endregion

        #region Properties

        public Int32 Octet1
        {
            get
            {
                return m_IPAddress[0];
            }
            set
            {
                m_IPAddress[0] = (byte)value;
                NotifyPropertyChanged("Octet1");
            }
        }

        public Int32 Octet2
        {
            get
            {
                return m_IPAddress[1];
            }
            set
            {
                m_IPAddress[1] = (byte)value;
                NotifyPropertyChanged("Octet2");
            }
        }

        public Int32 Octet3
        {
            get
            {
                return m_IPAddress[2];
            }
            set
            {
                m_IPAddress[2] = (byte)value;
                NotifyPropertyChanged("Octet3");
            }
        }

        public Int32 Octet4
        {
            get
            {
                return m_IPAddress[3];
            }
            set
            {
                m_IPAddress[3] = (byte)value;
                NotifyPropertyChanged("Octet4");
            }
        }


        public static readonly DependencyProperty IPAddressObjectProperty = DependencyProperty.Register("IPAddressObject", typeof(IPAddress), typeof(IPAddressTextBox), new PropertyMetadata(IPAddress.Parse("127.0.0.1"), new PropertyChangedCallback(UpdateIPAddressObjectProperty)));


        private static void UpdateIPAddressObjectProperty(DependencyObject ipAddressControl, DependencyPropertyChangedEventArgs e)
        {
            (ipAddressControl as IPAddressTextBox).IPAddressObject = (IPAddress)e.NewValue;
        }

      //  public static readonly DependencyProperty IPAddressObjectProperty = DependencyProperty.Register("IPAddressObject", typeof(IPAddress), typeof(IPAddressTextBox), new UIPropertyMetadata(null));



        public IPAddress IPAddressObject
        {
            get
            {
                return new IPAddress(m_IPAddress);
            }
            set
            {
                m_IPAddress = value.GetAddressBytes();
                NotifyPropertyChanged("Octet1");
                NotifyPropertyChanged("Octet2");
                NotifyPropertyChanged("Octet3");
                NotifyPropertyChanged("Octet4");
            }
        }

        public IPClass AddressClass
        {
            get
            {
                return m_AddressClass;
            }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return String.Format("{0}.{1}.{2}.{3}", Octet1, Octet2, Octet3, Octet4);
        }


        #endregion

        #region Events

        private void TextOctet_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void TextOctet_TextChanged(object sender, TextChangedEventArgs e)
        {

            String outString = String.Empty;

            String[] text = Regex.Split(((TextBox)sender).Text, "");
            foreach (string s in text)
                if (!Regex.IsMatch(s, "[^0-9]"))
                    outString += s;

            if (String.IsNullOrWhiteSpace(outString))
                outString = "";
            else if (int.Parse(outString) > 255)
                outString = "255";

            ((TextBox)sender).Text = outString;
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();
        }

        private void FirstOctet_TextChanged(object sender, TextChangedEventArgs e)
        {

            String outString = String.Empty;

            String[] text = Regex.Split(((TextBox)sender).Text, "");
            foreach (string s in text)
                if (!Regex.IsMatch(s, "[^0-9]"))
                    outString += s;

            if (String.IsNullOrWhiteSpace(outString))
                outString = "";
            else if (int.Parse(outString) > 255)
                outString = "255";
            else
            {
                int octect = int.Parse(outString);

                // Determine Address Class
                if (octect >= 0 && octect <= 127)
                {
                    // Class A
                    m_AddressClass = IPClass.ClassA;
                    NotifyClassChanged(m_AddressClass);
                }
                else if (octect >= 128 && octect <= 191)
                {
                    // Class B
                    m_AddressClass = IPClass.ClassB;
                    NotifyClassChanged(m_AddressClass);
                }
                else if (octect >= 192 && octect <= 223)
                {
                    // Class C
                    m_AddressClass = IPClass.ClassC;
                    NotifyClassChanged(m_AddressClass);
                }
            }


            ((TextBox)sender).Text = outString;
            ((TextBox)sender).GetBindingExpression(TextBox.TextProperty).UpdateSource();

        }

        private void TextOctet_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.SelectAll();
        }

        private void TextOctetDecimalArrows_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Decimal)
            {
                TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                if (keyboardFocus != null)
                {
                    keyboardFocus.MoveFocus(tRequest);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Right && ((TextBox)sender).Name != "TextOctet4")
            {
                if (((TextBox)sender).CaretIndex == ((TextBox)sender).Text.Length)
                {
                    TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                    UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                    if (keyboardFocus != null)
                    {
                        keyboardFocus.MoveFocus(tRequest);
                    }

                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Left && ((TextBox)sender).Name != "TextOctet1")
            {
                if (((TextBox)sender).CaretIndex == 0)
                {
                    TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Previous);
                    UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                    if (keyboardFocus != null)
                    {
                        keyboardFocus.MoveFocus(tRequest);
                    }

                    e.Handled = true;
                }
            }
        }

        private void TextOctetArrowsOnly_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right && ((TextBox)sender).Name != "TextOctet4")
            {
                if (((TextBox)sender).CaretIndex == ((TextBox)sender).Text.Length)
                {
                    TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Next);
                    UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                    if (keyboardFocus != null)
                    {
                        keyboardFocus.MoveFocus(tRequest);
                    }

                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Left && ((TextBox)sender).Name != "TextOctet1")
            {
                if (((TextBox)sender).CaretIndex == 0)
                {
                    TraversalRequest tRequest = new TraversalRequest(FocusNavigationDirection.Previous);
                    UIElement keyboardFocus = Keyboard.FocusedElement as UIElement;

                    if (keyboardFocus != null)
                    {
                        keyboardFocus.MoveFocus(tRequest);
                    }

                    e.Handled = true;
                }
            }
        }

        private void TextOctet1_LostFocus(object sender, RoutedEventArgs e)
        {
            if (String.IsNullOrWhiteSpace(((TextBox)sender).Text))
                ((TextBox)sender).Text = "0";
        }

        #endregion
    }
}
