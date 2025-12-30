#region Using directives
using System;
using UAManagedCore;
using OpcUa = UAManagedCore.OpcUa;
using FTOptix.UI;
using FTOptix.NativeUI;
using FTOptix.HMIProject;
using FTOptix.CoreBase;
using FTOptix.System;
using FTOptix.Retentivity;
using FTOptix.NetLogic;
using FTOptix.SerialPort;
using FTOptix.Core;
using FTOptix.RAEtherNetIP;
using FTOptix.CommunicationDriver;
using System.Data;
#endregion

public class PLCClockSync : BaseNetLogic
{
    public override void Start()
    {
        variableSynchronizer = new RemoteVariableSynchronizer();
        try
        {
            // Get variables which are externally linked to PLC tags
            yearPLC = LogicObject.GetVariable("PLC_Year");
            monthPLC = LogicObject.GetVariable("PLC_Month");
            dayPLC = LogicObject.GetVariable("PLC_Day");
            hourPLC = LogicObject.GetVariable("PLC_Hour");
            minutePLC = LogicObject.GetVariable("PLC_Minute");
            secondPLC = LogicObject.GetVariable("PLC_Second");
            microsecPLC = LogicObject.GetVariable("PLC_Milisecond");

            // Add them to variable synchronizer so their values are always read
            variableSynchronizer.Add(yearPLC);
            variableSynchronizer.Add(monthPLC);
            variableSynchronizer.Add(dayPLC);
            variableSynchronizer.Add(hourPLC);
            variableSynchronizer.Add(minutePLC);
            variableSynchronizer.Add(secondPLC);
            variableSynchronizer.Add(microsecPLC);
        }
        catch (Exception ex)
        {
            Log.Error($"{LogicObject.BrowseName}", $"{ex.Message}");
        }

        minutePLC.VariableChange += CheckTime;
    }

    public override void Stop()
    {
        minutePLC.VariableChange -= CheckTime;
    }

    private void CheckTime(object sender, VariableChangeEventArgs e)
    {
        DateTime currentSysTime = (DateTime)LogicObject.GetVariable("SystemTime").Value;
        Log.Info($"SysTime: {currentSysTime.Second}  PLC Time: {secondPLC.Value}");
        if ((currentSysTime.Second >= 30 && currentSysTime.Second < 55) || (currentSysTime.Second < 30 && currentSysTime.Second > 4)) // +- 5 second tolerance
            SetTime();
    }

    [ExportMethod]
    public void SetTime()
    {
        DateTime dt = new DateTime(yearPLC.Value, monthPLC.Value, dayPLC.Value, hourPLC.Value, minutePLC.Value, secondPLC.Value, (microsecPLC.Value/1000));
        LogicObject.GetVariable("SystemTime").Value = dt;
    }

    private IUAVariable yearPLC;
    private IUAVariable monthPLC;
    private IUAVariable dayPLC;
    private IUAVariable hourPLC;
    private IUAVariable minutePLC;
    private IUAVariable secondPLC;
    private IUAVariable microsecPLC;
    private RemoteVariableSynchronizer variableSynchronizer;
}
