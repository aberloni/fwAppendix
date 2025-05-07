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

		public void log()
		{
			Debug.Log("Device model: " + deviceModel);
			Debug.Log("Device name: " + deviceName);
			Debug.Log("Device type: " + deviceType);
			Debug.Log("Graphics device name: " + graphicsDeviceName);
			Debug.Log("Graphics device vendor: " + graphicsDeviceVendor);
			Debug.Log("Graphics device version: " + graphicsDeviceVersion);
			Debug.Log("OS: " + operatingSystem);
			Debug.Log("Processor count: " + processorCount);
			Debug.Log("Processor type: " + processorType);
			Debug.Log("Memory size: " + systemMemorySize);
		}

		/// <summary>
		/// _Data/hardware.dump
		/// </summary>
		public void dump(string subPath = "")
		{
			string path = System.IO.Path.Combine(Application.dataPath, subPath, "hardware.dump");
			System.IO.StreamWriter streamWriter = new(path, false);
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Window/Hardware/log")]
		static void logHardware()
		{
			new Hardware().log();
		}
#endif

	}

}