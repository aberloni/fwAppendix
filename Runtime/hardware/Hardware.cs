using UnityEngine;

namespace fwp.hardware
{

	[System.Serializable]
	public class Hardware
	{

		string deviceModel;
		string deviceName;
		DeviceType deviceType;
		string graphicsDeviceName;
		string graphicsDeviceVendor;
		string graphicsDeviceVersion;

		string operatingSystem;
		int processorCount;
		string processorType;
		int systemMemorySize;

		public Hardware()
		{
			readLocals();
		}

		public void readLocals()
		{
			deviceModel = SystemInfo.deviceModel;
			deviceName = SystemInfo.deviceName;
			deviceType = SystemInfo.deviceType;
			graphicsDeviceName = SystemInfo.graphicsDeviceName;
			graphicsDeviceVendor = SystemInfo.graphicsDeviceVendor;
			graphicsDeviceVersion = SystemInfo.graphicsDeviceVersion;

			operatingSystem = SystemInfo.operatingSystem;
			processorCount = SystemInfo.processorCount;
			processorType = SystemInfo.processorType;

			systemMemorySize = SystemInfo.systemMemorySize;
		}

		public string stringify()
		{
			string ret = "<b>[HARDWARE]</b>";
			ret += "\nDevice model: " + deviceModel;
			ret += "\nDevice name: " + deviceName;
			ret += "\nDevice type: " + deviceType;
			ret += "\nGraphics device name: " + graphicsDeviceName;
			ret += "\nGraphics device vendor: " + graphicsDeviceVendor;
			ret += "\nGraphics device version: " + graphicsDeviceVersion;
			ret += "\nOS: " + operatingSystem;
			ret += "\nProcessor count: " + processorCount;
			ret += "\nProcessor type: " + processorType;
			ret += "\nMemory size: " + systemMemorySize;
			return ret;
		}

		public void log()
		{
			Debug.Log(stringify());
		}

		/// <summary>
		/// _Data/hardware.dump
		/// </summary>
		public void dump(string subPath = "")
		{
			string path = System.IO.Path.Combine(Application.dataPath, subPath, "hardware.dump");
			var sw = new System.IO.StreamWriter(path, false);
			sw.WriteLine(deviceName);
			sw.WriteLine(System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
			sw.WriteLine(stringify());
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Window/Hardware/log")]
		static void miLogHardware()
		{
			new Hardware().log();
		}
		[UnityEditor.MenuItem("Window/Hardware/dump")]
		static void miDumpHardware()
		{
			new Hardware().dump();
		}
#endif

	}

}