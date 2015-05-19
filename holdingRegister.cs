using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace WpfMdbClient
{
   public class holdingRegister:ViewModelBase
    {
       private ushort _value;
       private ushort _oldvalue;
       private DateTime _update;
       private ushort _register;

       public holdingRegister(ushort register, ushort value)
       {
           _update = DateTime.Now;
           _register = register;
           _value = value;
           _oldvalue = value;
       }

       public ushort register
       { get { return _register; } private set { _register = value; } }

       public ushort value
       { get { return _value; } private set { _value = value; } }

       public ushort oldvalue
       { get { return _oldvalue; } private set { _oldvalue = value; } }

       public DateTime update
       { get { return _update; } private set { _update = value; } }


       public void UpdateValue(ushort data)
       {

           UpdateValue(DateTime.Now, data);
       }

       public void UpdateValue(DateTime updatetime, ushort data)
       {
           this.oldvalue = this.value;
           this.value = data;
           this.update = updatetime;

           RaisePropertyChanged("value");
           RaisePropertyChanged("oldvalue");
           RaisePropertyChanged("update");
       }

    }
}
