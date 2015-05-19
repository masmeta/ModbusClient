using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Modbus;

namespace WpfMdbClient
{
    public class MdbMaster
    {

         byte _identifier;
         int _port;
         IPAddress _address;
         Boolean _isConnected;
         Boolean _isrunningCyclic;
         Boolean _isprogress;
         Boolean _isbusy;
         TcpClient  TcpMaster;
         Modbus.Device.ModbusIpMaster _mdbMaster;
         int _startRegister; int _size;

         public MdbMaster(byte id,string ipAddress, int startRegister, int size)
         {
             _identifier = id;
             _port = 502;
             _address = System.Net.IPAddress.Parse(ipAddress);
             _startRegister = startRegister;
             _size = size;
         }



        public bool isConnected {get{return _isConnected;}}
        public bool isRunningCycle { get { return _isrunningCyclic; } }
        public bool isBusy { get { return _isbusy; } }
        public bool isProgress { get { return _isprogress; } }


        public bool startDialog()
        {
            try
            {
                if (!_isConnected)
                {
                    TcpMaster = new TcpClient(_address.ToString(), _port);
                    if (TcpMaster.Connected)
                    {
                        _isprogress = true;
                        _mdbMaster = Modbus.Device.ModbusIpMaster.CreateIp(TcpMaster);
                        
                        _isConnected = true;
                    }
                }
            }
            catch (Exception)
            {
                _isConnected = false;
                throw;
            }
            return true;
        }

        public bool stopDialog()
        {
            try
            {
                if (_isConnected)
                {
                    TcpMaster.Close();
                    
                    _isConnected = false;
                }
            }
            catch (Exception)
            {
                _isConnected = false;
                throw;
            }
            return true;
        }



        public bool ReadHoldingRegisters(List<holdingRegister> data)
        {
            try
            {
                if (!_isConnected) return false;
                ushort[] usData;
                usData = _mdbMaster.ReadHoldingRegisters((ushort)_startRegister, (ushort)_size);
                ushort val = (ushort)(DateTime.Now.Millisecond / 10);
                WriteHoldingRegisters((ushort)_startRegister, val);
                ushort reg = (ushort)_startRegister;
                    foreach(ushort v in usData)
                    {
                        data.Add(new holdingRegister(reg, v));
                        reg++;
                    }
            }
            catch (Exception ex)
            {
                
                throw;
            }
          

            return true;
        }

        public bool WriteHoldingRegisters(ushort Register, ushort value)
        {
            try
            {
                if (!_isConnected) return false;
                _mdbMaster.WriteSingleRegister(Register, value);
                
            }
            catch (Exception ex)
            {

                throw;
            }


            return true;
        }


    }
}
