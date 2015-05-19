using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Linq;
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Modbus.Data;
using System.Net;

namespace WpfMdbClient.ViewModel
{
	/// <summary>
	/// This class contains properties that the main View can data bind to.
	/// <para>
	/// Use the <strong>mvvminpc</strong> snippet to add bindable properties to this ViewModel.
	/// </para>
	/// <para>
	/// You can also use Blend to data bind with the tool's support.
	/// </para>
	/// <para>
	/// See http://www.galasoft.ch/mvvm
	/// </para>
	/// </summary>
	public class MainViewModel : ViewModelBase
	{

        public RelayCommand Disconnect { get; private set; }
        public RelayCommand Connect { get; private set; }

		private ObservableCollection<holdingRegister> _holdingRegList;
		private readonly ICollectionView _collectionHoldingRegView;
        private string _ipAddress;
		private int _startRegister;
		private int _endRegister;
        private bool _firstconnect;
        private MdbMaster _client;
        private DispatcherTimer _grabTimer = null;
		/// <summary>
		/// Initializes a new instance of the MainViewModel class.
		/// </summary>
		public MainViewModel()
		{
			////if (IsInDesignMode)
			////{
			////    // Code runs in Blend --> create design time data.
			////}
			////else
			////{
			////    // Code runs "for real"
			////}
			_ipAddress = "127.0.0.1";
			_holdingRegList = new ObservableCollection<holdingRegister>();
			this._collectionHoldingRegView = CollectionViewSource.GetDefaultView(_holdingRegList);

			if (this._collectionHoldingRegView == null)
				throw new NullReferenceException("collection device View");
			this._collectionHoldingRegView.CurrentChanged += _collectionHoldingRegView_CurrentChanged;

            Connect = new RelayCommand( _executeConnect, _canConnect);
            Disconnect = new RelayCommand(_executeDisconnect, _canDisconnect);
		}

		void _collectionHoldingRegView_CurrentChanged(object sender, EventArgs e)
		{
			RaisePropertyChanged("SelectedRegister");
            RaisePropertyChanged("HoldingRegister");
		}


		public ObservableCollection<holdingRegister> HoldingRegister
		{
			get { return _holdingRegList; }
		}

		public holdingRegister selectedDevice
		{
			get { return _collectionHoldingRegView.CurrentItem as holdingRegister; }
		}

        public string IpAddress { get { return _ipAddress; } set {  _ipAddress = value; RaisePropertyChanged("IpAddress");} }
		public int startRegister { get { return _startRegister; } set { if (value != _startRegister) { _startRegister = value; RaisePropertyChanged("startRegister"); } } }
		public int sizeToRead { get { return _endRegister; } set { if (value != _endRegister) { _endRegister = value; RaisePropertyChanged("endRegister"); } } }


        private bool _canDisconnect()
        {
            if (_client == null) return false;
            return _client.isConnected;
        }

        private bool _canConnect()
        {
            if (_client == null) return true;
            return !_client.isConnected;
        }

        private void _executeConnect()
        {
           
            _client = new MdbMaster(1, IpAddress, _startRegister, _endRegister);
            _client.startDialog();
            _initGrabTimer();
            Connect.RaiseCanExecuteChanged();
        }

        private void _executeDisconnect()
        {
            _firstconnect = false;
            _grabTimer.Stop();
            _grabTimer.Tick -= new EventHandler(_grabTimer_Tick);
            _client.stopDialog();
            _client = null;

            Disconnect.RaiseCanExecuteChanged();
        }


        /// <summary>
        /// Init grab timer
        /// </summary>
        private void _initGrabTimer()
        {

            _grabTimer = new DispatcherTimer();
            _grabTimer.Tick += new EventHandler(_grabTimer_Tick);
            _grabTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _firstconnect = true;
            _grabTimer.Start();
        }


        /// <summary>
        /// Action
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void _grabTimer_Tick(object sender, EventArgs e)
        {

            _grabTimer.Stop();
            List<holdingRegister> tmp=new List<holdingRegister>();
            _client.ReadHoldingRegisters(tmp);

            if (_firstconnect)
            {
                _holdingRegList.Clear();
                _firstconnect = false;
                foreach (holdingRegister v in tmp)
                    _holdingRegList.Add(v);
            }
            else
            {
                DateTime upTime = DateTime.Now;
                for(int i = 0; i <_holdingRegList.Count;i++)
                {
                    var reg = from x in tmp where (x.register == _holdingRegList[i].register) & (x.value != _holdingRegList[i].value) select x;
                    if (reg != null)
                    {
                        foreach (holdingRegister it in reg)
                        {
                            _holdingRegList[i].UpdateValue(upTime, it.value);
                            RaisePropertyChanged("HoldingRegister");
                        }
                    }
                }
            }
            _grabTimer.Start();
        }

	}
}