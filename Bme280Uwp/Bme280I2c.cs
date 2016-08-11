using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace Bme280Uwp
{
    /// <summary>
    /// I2C接続設定（CSB LOW）にしたBME280からの読み取りを行う。
    /// </summary>
    public class Bme280I2c : ABme280
    {
        private I2cDevice _bme280Ic2;

        private readonly string I2C_CONTROLLER_NAME;
        private readonly int I2C_ADDR;

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="i2cControllerName">Raspberry Pi: "I2C1"</param>
        /// <param name="i2cAddress">SDO LOW: 0x76, SDO HIGH: 0x77</param>
        /// <param name="updatePeriodMillis">XXX BME280向けに最適化する</param>
        public Bme280I2c(string i2cControllerName, int i2cAddress, int updatePeriodMillis)
            : base(updatePeriodMillis)
        {
            I2C_CONTROLLER_NAME = i2cControllerName;
            I2C_ADDR = i2cAddress;
        }

        protected override async Task InitializeInterface()
        {
            string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
            var dis = await DeviceInformation.FindAllAsync(aqs);

            var settings = new I2cConnectionSettings(I2C_ADDR);
            settings.BusSpeed = I2cBusSpeed.FastMode;
            _bme280Ic2 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
            // XXX これでは有無はチェックできない
            //if (_bme280Ic2 == null)
            //{
            //    throw new Exception(string.Format("Device was not found. {0}, {1}", I2C_CONTROLLER_NAME, I2C_ADDR));
            //}
        }

        protected override void WriteRegister(byte address, byte data)
        {
            byte[] writeBuf = new byte[] { address, data };
            _bme280Ic2.Write(writeBuf);
        }

        protected override Bme280CompenstionParameter ReadTrim()
        {
            byte[] writeBuf;
            var from0x88 = new byte[26];
            var from0xe1 = new byte[16];

            writeBuf = new byte[] { 0x88 };
            _bme280Ic2.WriteRead(writeBuf, from0x88);

            writeBuf = new byte[] { 0xe1 };
            _bme280Ic2.WriteRead(writeBuf, from0xe1);

            var calib = new Bme280CompenstionParameter(from0x88, from0xe1);
            return calib;
        }

        protected override Bme280Data ReadData()
        {
            byte[] writeBuf = new byte[] { 0xF7 };
            byte[] readBuf = new byte[8];
            _bme280Ic2.WriteRead(writeBuf, readBuf);

            var pres_raw = (readBuf[0] << 12) | (readBuf[1] << 4) | (readBuf[2] >> 4);
            var temp_raw = (readBuf[3] << 12) | (readBuf[4] << 4) | (readBuf[5] >> 4);
            var hum_raw = (readBuf[6] << 8) | readBuf[7];

            return Build(temp_raw, pres_raw, hum_raw);
        }
    }
}
