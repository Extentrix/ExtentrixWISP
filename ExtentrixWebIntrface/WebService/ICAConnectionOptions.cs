using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;


/// <summary>
/// Manage Session Pereferences
/// </summary>
[XmlTypeAttribute(Namespace = NamespaceConstants.v3)]

    public class ICAConnectionOptions
    {
        public ICAConnectionOptions() {
            encryptionLevel = EncryptionLevel._defaultLevel;
            soundQuality = SoundQuality.MediumSoundQuality;
            windowSize = WindowSize._seamlessWindow;
            windowColor = WindowColor._defaultColors;
            connectionSpeed = ConnectionSpeed.Custom;
            EnablePrinterMapping = false;
            EnablePDADeviceSupport = false;
            UseDataCompression = false;
            QueueMouseMovementsAndKeystrokes = false;
            EnableSessionReliability = false;
            EnableSound = false;

        }
        
        public EncryptionLevel encryptionLevel;
        public SoundQuality soundQuality;
        public WindowSize windowSize;
        public WindowColor windowColor;
        public TransparentKeyPassthrough keyboard;
        public ConnectionSpeed connectionSpeed;
        public bool EnablePDADeviceSupport ;
        public bool EnablePrinterMapping;
        public bool UseDataCompression ;
	    public bool useDiskCacheForBitmaps ;
	    public bool QueueMouseMovementsAndKeystrokes ;
	    public bool EnableSessionReliability ;
        public bool EnableSound;
        public int screenWidth;
        public int screenHight;
        public int screenSizePercent;
       // bool SoundCustomDefault;
       // bool MouseClickFeedback ;
       // bool LocalTextEcho;
    }

    public enum ConnectionSpeed
    {
        Custom = 0,
        High = 1,
        MediumHigh = 2,
        Medium = 3,
        Low = 4
    }


    public enum SpeedScreenLatencyReduction
    {
        Auto = 0,
        On = 1,
        Off = 2
    }

    public enum TransparentKeyPassthrough
    {
        OnLocalComputer = 0,
        OnRemoteComputer = 1,
        OnFullScreenOnly = 2
    }
    public enum SoundQuality
    {
        MediumSoundQuality = 0,
        LowSoundQuality = 1,
        HighsoundQuality = 2
    }
    public enum EncryptionLevel
    {
        _defaultLevel = 0,
        _basic = 1,
        _128bitForLogonOnly = 2,
        _40bit = 3,
        _56bit = 4,
        _128bit = 5
    }
    //
    //
    //DesiredColor=1 | 2 | 4 | 8
    //DesiredColor sets the color palette depth. Use 1 for 16-color and 2 for 256-color. The default is 2 (256-color). Use 4 for High Color. For True Color, use 8.
    //

    public enum WindowColor
    {
        _defaultColors = 0,
        _256colors = 1,
        _16color = 2,
        _highColor = 3,
        _trueColor
    }

    public enum WindowSize
    {
        _640x480 = 0,
        _800x600 = 1,
        _1024x768 = 2,
        _1280x1024 = 3,
        _1600x1200 = 4,
        _custom = 5,
        _PercentOfScreenSize = 6,
        _fullScreen = 7,
        _seamlessWindow = 8
    }



    
