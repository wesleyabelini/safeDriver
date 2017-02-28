
using System;
using Android;
using Android.App;
using Android.Bluetooth;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Java.Lang;
using Android.Bluetooth.LE;
using Java.Util;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Runtime;

namespace safeDriver
{
    [Activity(Label = "safeDriver", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public TextView text1;
        public TextView text2;
        public TextView text3;

        public static string tag = "safeDriver";
        public static string  mac = "E8:6F:09:D7:61:4F";

        BluetoothGatt gatt=null;
        BluetoothAdapter adapter= BluetoothAdapter.DefaultAdapter;
        BluetoothDevice device;

        public GattStatus status;
        public ProfileState state = ProfileState.Connecting;

        protected GattCallback gattCallBack = new GattCallback();
        Service servico = new Service();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.Main);

            text1 = FindViewById<TextView>(Resource.Id.textView1);
        }

        protected override void OnStart()
        {
            base.OnStart();

            text1.Text = "Desconectado";

            connect();
            gattCallBack.OnConnectionStateChange(gatt, status, ProfileState.Connected);

            gattCallBack.OnServicesDiscovered(gatt, status);
            //gattCallBack.OnCharacteristicRead(gatt, characteristic, status);
            
            var serviceList = gatt.Services;
            //var characteristicList = service.GetCharacteristic(characteristic.Uuid);

            //gatt.SetCharacteristicNotification(characteristic, true);

            //gattCallBack.OnReadRemoteRssi(gatt, 0, status);
            //gattCallBack.OnCharacteristicChanged(gatt, characteristic);
            //text1.Text ="RSSI " + gattCallBack.textRSSI;

            //text1.Text = "Characteristic " + characteristicList;
        }

        public static void stopScan(ScanCallback callback)
        {
            BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
            if (adapter == null)
            {
                Log.Debug(tag, "BluetoothAdapter is null");
            }

            BluetoothLeScanner scaner = adapter.BluetoothLeScanner;
            if (scaner == null)
            {
                Log.Debug(tag, "BluetoothLeScanner is null");
            }

            scaner.StopScan(callback);
        }

        public void connect()
        {
            if (adapter == null)
            {
                Log.Debug(tag, "BluetoothAdapter is null");
            }

            BluetoothLeScanner scaner = adapter.BluetoothLeScanner;
            if (scaner == null)
            {
                Log.Error(tag, "BluetoothLeScanner is null");
            }

            try
            {
                device = adapter.GetRemoteDevice(mac);
                if (device == null)
                {
                    Log.Debug(tag, "Device não encontrado");
                }

                gatt = device.ConnectGatt(this, true, gattCallBack);
            }
            catch (System.Exception ex)
            {
                Log.Debug(tag, ex.ToString());
            }

        }

        protected class GattCallback: BluetoothGattCallback
        {
            public string textRSSI;
            public string textCharacteristic;

            public override void OnConnectionStateChange(BluetoothGatt gatt, [GeneratedEnum] GattStatus status, [GeneratedEnum] ProfileState newState)
            {
                base.OnConnectionStateChange(gatt, status, newState);

                if(status == GattStatus.Success)
                {
                    if(newState == ProfileState.Connected)
                    {
                        gatt.DiscoverServices();
                    }
                    else if(newState == ProfileState.Disconnected)
                    {
                        Log.Debug(tag, "GATT DESCONECTADO");
                        gatt.Close();
                        gatt = null;
                    }
                }
            }

            public override void OnCharacteristicRead(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic, [GeneratedEnum] GattStatus status)
            {
                base.OnCharacteristicRead(gatt, characteristic, status);

                if(status==GattStatus.Success)
                {
                    
                    gatt.ReadCharacteristic(characteristic);
                    byte[] valor = characteristic.GetValue();
                    textCharacteristic = gatt.ReadCharacteristic(characteristic).ToString();
                    Log.Debug(tag, "CHARACTERISTIC: " + valor);
                }
                else
                {
                    Log.Debug(tag, "characteristic GET VALUE IS NULL");
                }
            }

            public override void OnCharacteristicChanged(BluetoothGatt gatt, BluetoothGattCharacteristic characteristic)
            {
                base.OnCharacteristicChanged(gatt, characteristic);

                byte[] data = characteristic.GetValue();
                string dataA = "";

                for(int i = 0; i < data.Length; i++)
                {
                    dataA += data[i].ToString();
                }

                Log.Debug(tag, "CHARACTERISTIC: " + characteristic.Uuid.ToString() + " CHANGED: " + dataA);
            }

            public override void OnReadRemoteRssi(BluetoothGatt gatt, int rssi, [GeneratedEnum] GattStatus status)
            {
                base.OnReadRemoteRssi(gatt, rssi, status);

                if(status == GattStatus.Success)
                {
                    gatt.ReadRemoteRssi();
                    textRSSI = rssi.ToString();
                    
                    Log.Debug(tag, "Remote RSSI: " + rssi);
                }
            }

            public override void OnServicesDiscovered(BluetoothGatt gatt, [GeneratedEnum] GattStatus status)
            {
                base.OnServicesDiscovered(gatt, status);

                foreach(BluetoothGattService service in gatt.Services)
                {
                    string uuid = service.Uuid.ToString().ToUpper();

                    if(uuid!="")
                    {
                        foreach(BluetoothGattCharacteristic characteristic in service.Characteristics)
                        {
                            string c_uuid = characteristic.Uuid.ToString().ToUpper();

                            if(c_uuid!="")
                            {
                                gatt.SetCharacteristicNotification(characteristic, true);

                                BluetoothGattDescriptor descriptor = new BluetoothGattDescriptor(UUID.FromString("00002A25-0000-1000-8000-00805F9B34FB"), GattDescriptorPermission.Read | GattDescriptorPermission.Write);
                                characteristic.AddDescriptor(descriptor);
                                gatt.SetCharacteristicNotification(characteristic, true);
                                gatt.ReadDescriptor(descriptor);
                                byte[] data = descriptor.GetValue();
                                descriptor.SetValue(data);
                                gatt.WriteDescriptor(descriptor);
                                byte[] chara = characteristic.GetValue();

                                if (data !=null)
                                {
                                    Log.Debug(tag, "VALOR AQUI: " + data.ToString());
                                    Log.Debug(tag, "Chara: " + chara[0].ToString());
                                }
                                Log.Debug(tag, "Chara: " + chara[0].ToString());
                                Log.Debug(tag, "VALOR AQUI: " + data.ToString());
                                

                                //Log.Debug(tag, "VALOR AQUI: " + data.ToString());
                            }
                        }
                    }
                }
            }
        }
    }
}

