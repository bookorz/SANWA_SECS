﻿using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Net;
//using System.Net.Sockets;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;
//using System.Messaging;
//using Secs4Net;

namespace SanwaSecsDll
{
    public class SanwaSecs
    {
        public SecsGem _secsGem;
        public int T3 { get { return _secsGem.T3; } set { _secsGem.T3 = value; } }
        public int T5 { get { return _secsGem.T5; } set { _secsGem.T5 = value; } }
        public int T6 { get { return _secsGem.T6; } set { _secsGem.T6 = value; } }
        public int T7 { get { return _secsGem.T7; } set { _secsGem.T7 = value; } }
        public int T8 { get { return _secsGem.T8; } set { _secsGem.T8 = value; } }

        //提供Interface給外部紀錄Log
        public ISecsGemLogger _logger;

        //轉換sml檔案
        public SecsMessageList _secsMessages;

        public SanwaBaseExec _baseExec;

        public string eqpSVFileName = null;
        public string eqpEventFileName = null;
        public string eqpECFileName = null;
        public string eqpAlarmFileName = null;
        public string eqpDVFileName = null;

        public Dictionary<string, SanwaSV> _svList = new Dictionary<string, SanwaSV>();        //search by Name 
        private Dictionary<string, SanwaSV> _svIDList = new Dictionary<string, SanwaSV>();       //search by ID   

        public Dictionary<string, SanwaEC> _ecList = new Dictionary<string, SanwaEC>();         //search by Name 
        private Dictionary<string, SanwaEC> _ecIDList = new Dictionary<string, SanwaEC>();       //search by ID 

        public Dictionary<string, SanwaAlarm> _alarmList = new Dictionary<string, SanwaAlarm>();    //search by Name 
        private Dictionary<string, SanwaAlarm> _alarmIDList = new Dictionary<string, SanwaAlarm>();  //search by ID 

        public Dictionary<string, SanwaDV> _dvList = new Dictionary<string, SanwaDV>();
        private Dictionary<string, SanwaDV> _dvIDList = new Dictionary<string, SanwaDV>();

        public Dictionary<string, SanwaEvent> _eventList = new Dictionary<string, SanwaEvent>();
        private Dictionary<string, SanwaEvent> _eventIDList = new Dictionary<string, SanwaEvent>();

        public Dictionary<string, SanwaRPTID> _reportList = new Dictionary<string, SanwaRPTID>();


        public List<string> _terminalMSGList = new List<string>();

        public Dictionary<string, LoadPort> _loadPortList = new Dictionary<string, LoadPort>();
        public Dictionary<string, LoadPortGroup> _loadPortGroupList = new Dictionary<string, LoadPortGroup>();
        
        public Dictionary<string, SanwaCarrier> _carrierList = new Dictionary<string, SanwaCarrier>();

        //
        //public BindingList<PrimaryMessageWrapper> recvBuffer = new BindingList<PrimaryMessageWrapper>();

        //連線狀態變化
        public delegate void ConnectionStateChanged();
        public event ConnectionStateChanged ConnectionStateChangedEvent;
        public void ConnectionChanged(object sender, ConnectionState e)
        {
            if (e == ConnectionState.Selected)
                SetSV(SVName.GEM_CONTROL_STATE, 2);

            ConnectionStateChangedEvent?.Invoke();
        }

        public event EventHandler<PrimaryMessageWrapper> PrimaryMessageReceivedEvent;
        public void OnPrimaryMessageReceivedCallback(PrimaryMessageWrapper e)
        {
            PrimaryMessageReceivedEvent?.Invoke(this, e);
        }
        public event EventHandler<CONTROL_STATE> ChangeControlStateEvent;

        public event EventHandler<E87_HostCommand> S3F17BindEvent;
        public event EventHandler<E87_HostCommand> S3F17CancelBindEvent;
        public event EventHandler<E87_HostCommand> S3F17CarrierNotificationEvent;
        public event EventHandler<E87_HostCommand> S3F17CancelCarrierNotificationEvent;
        public event EventHandler<E87_HostCommand> S3F17ProceedWithCarrierEvent;
        public event EventHandler<E87_HostCommand> S3F17CancelCarrierEvent;
        public event EventHandler<E87_HostCommand> S3F17CancelCarrierAtPortEvent;
        public event EventHandler<E87_HostCommand> S3F17CarrierReCreateEvent;
        public event EventHandler<E87_HostCommand> S3F17CarrierReleaseEvent;
        public event EventHandler<E87_HostCommand> S3F17CarrierOutEvent;
        public event EventHandler<E87_HostCommand> S3F17CancelCarrierOutEvent;
        public event EventHandler<E87_HostCommand> S3F17CarrierInEvent;

        public event EventHandler<E87_HostCommand> S3F19CancelAllCarrierEvent;

        public event EventHandler<E87_HostCommand> S3F21AutoModeEvent;
        public event EventHandler<E87_HostCommand> S3F21ManualModeEvent;

        public event EventHandler<E87_HostCommand> S3F25InServiceEvent;
        public event EventHandler<E87_HostCommand> S3F25OutOfServiceEvent;
        public event EventHandler<E87_HostCommand> S3F25ReserveAtPortEvent;
        public event EventHandler<E87_HostCommand> S3F25CancelReserveAtPortEvent;
        public event EventHandler<E87_HostCommand> S3F25AutoModeEvent;
        public event EventHandler<E87_HostCommand> S3F25ManualModeEvent;

        public event EventHandler<E87_HostCommand> S3F27AutoModeEvent;
        public event EventHandler<E87_HostCommand> S3F27ManualModeEvent;

        public event EventHandler<SecsMessageCommand> S10F3TerminalMessageEvent;
        public event EventHandler<SecsMessageCommand> S10F5TerminalMessageEvent;


        //連線是否成功
        public bool IsConnected = false;

        //TCP/IP 相關參數
        public bool IsActiveMode { get; set; }
        public string IpAddress;
        public int Port;
        public int DecoderBufferSize;

        //Device ID
        public int DeviceId;

        //連線狀態
        public ConnectionState State { get { return _secsGem.State; }}
        //S1F1
        public string strMDLN {
                get =>_baseExec?._mdln;
                set {
                        if (_baseExec != null) _baseExec._mdln = value;
                        _svList.TryGetValue(SVName.GEM_MDLN, out SanwaSV svObj);
                        svObj._value = value;

                }
        }
        public string strSOFTREV
        {
            get => _baseExec?._softRev;
            set {
                    if (_baseExec != null) _baseExec._softRev = value;
                    _svList.TryGetValue(SVName.GEM_SOFTREV, out SanwaSV svObj);
                    svObj._value = value;
            }
        }
        public CONTROL_STATE _currentState
        {
            get { return _baseExec._currentState; }
            set { _baseExec._currentState = value; }
        }
        public bool ChangeLoadPortState(LoadPort lpObj, E87_LPTS newState)
        {
            return _baseExec.ChangeLoadPortState(lpObj, newState);
        }
        public E87RETURN_AM ChangeAccessMode(LoadPort lpObj, E87_AM newAccessMode)
        {
            return _baseExec.ChangeAccessMode(lpObj, newAccessMode);
        }

        public bool ChangeReserviceState(LoadPort lpObj, E87_RS ReserviceState)
        {
            return _baseExec.ChangeReserviceState(lpObj, ReserviceState);
        }

        public void LoadPortCarrierAssociated(LoadPort lpObj, SanwaCarrier CarrierObj, E87_ASSOCIATED AssociatedState)
        {
            _baseExec.LoadPortCarrierAssociated(lpObj, CarrierObj, AssociatedState);
        }

        public bool CarrierIDStatusChange(SanwaCarrier carrier, E87_CIDS IDStatus)
        {
            return _baseExec.CarrierIDStatusChange(carrier, IDStatus);
        }

        public bool CarrierSlotMapStatusChange(SanwaCarrier carrier, E87_CSMS slotMapStatus)
        {
            return _baseExec.CarrierSlotMapStatusChange(carrier, slotMapStatus);
        }

        public bool CarrierAccessingStatus(SanwaCarrier carrier, E87_CAS AccessingStatus)
        {
            return _baseExec.CarrierAccessingStatus(carrier, AccessingStatus);
        }


        public int _dataID {
            get {return _baseExec == null ? 1 : _baseExec._dataID; }
            //get { return _baseExec._dataID; }
            set { if (_baseExec != null) _baseExec._dataID = value;}
        }
        //public object EventName { get; set; }
        //public string _terminalText = "Terminal Text!!!";
        public SanwaSecs()
        {
            _secsGem = null;
            IsActiveMode = true;
            IpAddress = "127.0.0.1";
            Port = 5000;
            DecoderBufferSize = 65535;
            DeviceId = 0;
            _logger = null;
            _secsMessages = null;
            _baseExec = null;

            //_baseExec = new SanwaBaseExec(_secsMessages);
        }
        public SanwaSecs(bool isActive, string ip, int port, 
            int receiveBufferSize = 0x4000, ISecsGemLogger logger = null, SecsMessageList secsMessages = null)
        {
            _secsGem = null;

            IsActiveMode = isActive;
            IpAddress = ip;
            Port = port;
            DecoderBufferSize = receiveBufferSize;
            DeviceId = 0;
            _logger = logger;
            _secsMessages = secsMessages;
            _baseExec = null;

            //_baseExec = new SanwaBaseExec(_secsMessages);
        }

        public void Initialize()
        {
            if (_baseExec == null)
            {
                _baseExec = new SanwaBaseExec(_secsMessages)
                {
                    _svList = _svList,
                    _svIDList = _svIDList,
                    _ecList = _ecList,
                    _ecIDList = _ecIDList,
                    _alarmList = _alarmList,
                    _alarmIDList = _alarmIDList,
                    _dvList = _dvList,
                    _dvIDList = _dvIDList,
                    _terminalMSGList = _terminalMSGList,
                    _eventList = _eventList,
                    _eventIDList = _eventIDList,
                    _reportList = _reportList,

                    _loadPortList = _loadPortList,
                    _loadPortGroupList = _loadPortGroupList,

                    _carrierList = _carrierList,

                    _logger = _logger
                };

                _baseExec.ChangeControlStateEvent += ChangeControlStateEvent;

                _baseExec.S3F17BindEvent += S3F17BindEvent;
                _baseExec.S3F17CancelBindEvent+= S3F17CancelBindEvent;
                _baseExec.S3F17CarrierNotificationEvent += S3F17CarrierNotificationEvent;
                _baseExec.S3F17CancelCarrierNotificationEvent += S3F17CancelCarrierNotificationEvent;
                _baseExec.S3F17ProceedWithCarrierEvent += S3F17ProceedWithCarrierEvent;
                _baseExec.S3F17CancelCarrierEvent += S3F17CancelCarrierEvent;
                _baseExec.S3F17CancelCarrierAtPortEvent += S3F17CancelCarrierAtPortEvent;
                _baseExec.S3F17CarrierReCreateEvent += S3F17CarrierReCreateEvent;
                _baseExec.S3F17CarrierReleaseEvent += S3F17CarrierReleaseEvent;

                _baseExec.S3F17CarrierOutEvent += S3F17CarrierOutEvent;
                _baseExec.S3F17CancelCarrierOutEvent += S3F17CancelCarrierOutEvent;
                _baseExec.S3F17CarrierInEvent += S3F17CarrierInEvent;

                _baseExec.S3F19CancelAllCarrierEvent += S3F19CancelAllCarrierEvent;

                _baseExec.S3F21AutoModeEvent+=S3F21AutoModeEvent;
                _baseExec.S3F21ManualModeEvent+=S3F21ManualModeEvent;

                _baseExec.S3F25InServiceEvent += S3F25InServiceEvent;
                _baseExec.S3F25OutOfServiceEvent += S3F25OutOfServiceEvent;
                _baseExec.S3F25ReserveAtPortEvent += S3F25ReserveAtPortEvent;
                _baseExec.S3F25CancelReserveAtPortEvent += S3F25CancelReserveAtPortEvent;
                _baseExec.S3F25AutoModeEvent += S3F25AutoModeEvent;
                _baseExec.S3F25ManualModeEvent += S3F25ManualModeEvent;

                _baseExec.S3F27AutoModeEvent += S3F27AutoModeEvent;
                _baseExec.S3F27ManualModeEvent += S3F27ManualModeEvent;

                _baseExec.S10F3TerminalMessageEvent += S10F3TerminalMessageEvent;
                _baseExec.S10F5TerminalMessageEvent += S10F5TerminalMessageEvent;

                LoadSVIDCSVFile(eqpSVFileName);
                LoadEventIDCSVFile(eqpEventFileName);
                LoadECCSVFile(eqpECFileName);
                LoadAlarmCSVFile(eqpAlarmFileName);
                LoadDVCSVFile(eqpDVFileName);

                _baseExec.Initialize();

                SVInitialize();

                _ecList.TryGetValue(ECName.GEM_INIT_COMM_STATE, out SanwaEC ecObj);
                {
                    if ("1"== ecObj._value.ToString())
                        Connect();
                }
            }
        }
        public void Connect()
        {
            _secsGem?.Dispose();

            _secsGem = new SecsGem(
                IsActiveMode,
                IPAddress.Parse(IpAddress),
                Port,
                DecoderBufferSize)
            { Logger = _logger, DeviceId = (ushort)DeviceId };

            _baseExec._secsGem = _secsGem;

            _secsGem.ConnectionChanged += ConnectionChanged;
            _secsGem.PrimaryMessageReceived += PrimaryMessageReceived;
            _secsGem.Start();

        }
        public void Disconnect()
        {

            _secsGem?.Dispose();
            _secsGem = null;

            IsConnected = false;
        }
        private bool CheckConnectState()
        {
            bool bRet = true;
            if (_secsGem.State != ConnectionState.Selected) bRet = false;

            

            if (!(_currentState == CONTROL_STATE.ON_LINE_LOCAL ||
                _currentState == CONTROL_STATE.ON_LINE_REMOTE))
                bRet = false;

            return bRet;
        }
        private async void PrimaryMessageReceived(object sender, PrimaryMessageWrapper e)
        {
            try
            {
                PROCESS_MSG_RESULT lResult = await _baseExec.ProcessMessage(e);

                if (lResult == PROCESS_MSG_RESULT.PROCESS_NOT_FINISH)
                {
                    OnPrimaryMessageReceivedCallback(e);
                }
                else if (lResult == PROCESS_MSG_RESULT.ALREADY_REPLIED)
                {
                    if (1 == e.Message.S && 15 == e.Message.F) //Request OFF-LINE(ROFL) S,H->E,reply
                    {
                        await S6F11Async(EventName.GEM_EQP_OFF_LINE);
                    }
                    else if (1 == e.Message.S && 17 == e.Message.F)  //Request ON-LINE(RONL) S,H->E,reply
                    {
                        if (CONTROL_STATE.ON_LINE_LOCAL == _currentState)
                            await S6F11Async(EventName.GEM_CONTROL_STATE_LOCAL);
                        if (CONTROL_STATE.ON_LINE_REMOTE == _currentState)
                            await S6F11Async(EventName.GEM_CONTROL_STATE_REMOTE);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Handle Primary SECS message Error", ex);
            }
        }
        public async Task ChangeToOffLineState()
        {
            if (_secsGem.State != ConnectionState.Selected)
                return;

            if (!(_currentState == CONTROL_STATE.ON_LINE_LOCAL ||
                _currentState == CONTROL_STATE.ON_LINE_REMOTE ||
                _currentState == CONTROL_STATE.HOST_OFF_LINE))
                return;
            
            if (_baseExec != null)
            {
                await S6F11Async(EventName.GEM_EQP_OFF_LINE);

                PROCESS_MSG_RESULT lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;     //搭配Function無實質意義
                SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();    //搭配Function無實質意義

                //切換狀態機
                _baseExec.ChangeControlState(CONTROL_STATE.EQUIPMENT_OFF_LINE,null, s6f11format,ref lResult);

                SetEV(ECName.GEM_INIT_CONTROL_STATE, 1);
            }

        }
        public async Task RequestToOnLine()
        {
            if (_secsGem.State != ConnectionState.Selected)
                return;

            if (_currentState != CONTROL_STATE.EQUIPMENT_OFF_LINE)
                return;


            SecsMessage s1f1format = _secsMessages[1, 1].FirstOrDefault();
            PROCESS_MSG_RESULT lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;

            if (s1f1format == null)
            {
                _logger.Error("ChangeToOnLineState s1f1format error");
                return;
            }

            _baseExec.ChangeControlState(CONTROL_STATE.ATTEMPT_ON_LINE, null, s1f1format, ref lResult);

            var s1f1 = new SecsMessage(1,1, s1f1format.Name);
            var s1f2 = await _secsGem.SendAsync(s1f1);

            if (s1f2 == null)
            {
                _logger.Error("ChangeToOnLineState Timeout");
                return;
            }

            if (s1f2.F != 2)
            {
                lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;     //搭配Function無實質意義
                SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();    //搭配Function無實質意義

                if(s6f11format != null)
                { 
                    //切換狀態機
                    _baseExec.ChangeControlState(CONTROL_STATE.EQUIPMENT_OFF_LINE, null, s6f11format, ref lResult);

                    await S6F11Async(EventName.GEM_EQP_OFF_LINE);
                }
                return;
            }
            else
            {
                _ecList.TryGetValue(ECName.GEM_ON_LINE_SUBSTATE, out SanwaEC sanwaEC);
                if (sanwaEC == null)
                {
                    _logger.Error("GEM_ON_LINE_SUBSTATE NO DEFINED");
                }
                else
                {
                    lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;     //搭配Function無實質意義
                    SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();    //搭配Function無實質意義

                    if (s6f11format != null)
                    {
                        //Host On-line 需求 4:On-line/Local, 5:ON-line/Remote
                        if ("4" == sanwaEC._defaultValue.ToString())
                        {
                            _baseExec.ChangeControlState(CONTROL_STATE.ON_LINE_LOCAL, null, s6f11format, ref lResult);
                            await S6F11Async(EventName.GEM_CONTROL_STATE_LOCAL);
                        }
                        else if ("5" == sanwaEC._defaultValue.ToString())
                        {
                            _baseExec.ChangeControlState(CONTROL_STATE.ON_LINE_REMOTE, null, s6f11format, ref lResult);
                            await S6F11Async(EventName.GEM_CONTROL_STATE_REMOTE);
                        }

                        SetEV(ECName.GEM_INIT_CONTROL_STATE, 2);
                    }
                }
            }
        }
        public async Task ChangeToOnLineLocalState()
        {
            if (_secsGem.State != ConnectionState.Selected)
                return;

            if (_currentState != CONTROL_STATE.ON_LINE_REMOTE)
                return;

            PROCESS_MSG_RESULT lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;     //搭配Function無實質意義
            SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();    //搭配Function無實質意義
            if (s6f11format != null)
            {
                _baseExec.ChangeControlState(CONTROL_STATE.ON_LINE_LOCAL, null, s6f11format, ref lResult);
                await S6F11Async(EventName.GEM_CONTROL_STATE_LOCAL);

                SetEV(ECName.GEM_INIT_CONTROL_STATE, 2);
            }
            else
            {
                _logger.Error("S6F11_NO_FIND");
            }
        }
        public async Task ChangeToOnLineRemoteState()
        {
            if (_secsGem.State != ConnectionState.Selected)
                return;

            if (_currentState != CONTROL_STATE.ON_LINE_LOCAL)
                return;

            PROCESS_MSG_RESULT lResult = PROCESS_MSG_RESULT.PROCESS_FINISH;     //搭配Function無實質意義
            SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();    //搭配Function無實質意義
            if (s6f11format != null)
            {
                _baseExec.ChangeControlState(CONTROL_STATE.ON_LINE_REMOTE, null, s6f11format, ref lResult);
                await S6F11Async(EventName.GEM_CONTROL_STATE_REMOTE);

                SetEV(ECName.GEM_INIT_CONTROL_STATE, 2);
            }
            else
            {
                _logger.Error("S6F11_NO_FIND");
            }
        }
        public async Task S1F1Async()
        {
            if (_baseExec == null) return;
            if (!CheckConnectState()) return;

            try
            {
                var s1f1 = _secsMessages[1,1].FirstOrDefault();
                var s1f2 = await _secsGem.SendAsync(s1f1);
            }
            catch (Exception ex)
            {
                _logger.Info("S1F1:" + ex.Message);
            }
        }
        public async Task S1F13Async()
        {
            if (_baseExec == null) return;
            if (!CheckConnectState()) return;

            try
            {
                SecsMessage s1f13, s1f13format;
                //根據SML的名稱(決定是Host或者是Client)
                s1f13format = IsActiveMode ? _secsMessages[1, 13, "CR"] : _secsMessages[1, 13, "CR_Host"];

                s1f13 = new SecsMessage(s1f13format.S, s1f13format.F, s1f13format.Name);
                if (IsActiveMode)
                {
                    s1f13 = s1f13format.ToSml().ToSecsMessage();

                    foreach (var id in s1f13.SecsItem.Items)
                    {
                        if (id.Format == SecsFormat.ASCII)
                        {
                            if (id.GetString().Equals("MDLN"))
                            {
                                id.SetString(strMDLN);
                            }
                            else if (id.GetString().Equals("SOFTREV"))
                            {
                                id.SetString(strSOFTREV);
                            }
                        }
                    }
                }

                SecsMessage s1f14 = await _secsGem.SendAsync(s1f13);
            }
            catch (Exception ex)
            {
                _logger.Info("S1F13:" + ex.Message);
            }
        }
        public async Task S2F17Async()
        {
            if (_baseExec == null) return;
            if (!CheckConnectState()) return;

            SecsMessage s2f17format = _secsMessages[2, 17].FirstOrDefault();

            _secsGem.SendAsync(s2f17format);

            SetSV(SVName.GEM_CLOCK, _baseExec.GetDateTime());
        }
        private async Task SendEventReportAsync(string eventName, bool annotated)
        {
            if (_baseExec == null) return;
            if (!CheckConnectState()) return;

            _eventList.TryGetValue(eventName, out SanwaEvent sanwaEvent);
            if (sanwaEvent == null) return;
            if (!sanwaEvent._enabled) return;

            _ecList.TryGetValue(ECName.GEM_DATAID_FORMAT, out SanwaEC sanwaEC);
            if (sanwaEC == null) return;

            _svList.TryGetValue(SVName.GEM_PREVIOUS_CEID, out SanwaSV sanwaSV);
            if(sanwaSV != null)
                SetSV(SVName.GEM_PREVIOUS_CEID, sanwaSV._id);

            //Data 累加
            _dataID = _dataID + 1;

            SanwaEC ecObj;

            SecsMessage s6f11format = _secsMessages[6, 11].FirstOrDefault();

            string ReplyMSG = _baseExec.GetMessageName(s6f11format.ToSml());

            ReplyMSG += "< L[3]\r\n";
            switch (sanwaEC._value.ToString())
            {
                case "1":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.I1, (sbyte)_dataID);
                    break;
                case "2":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.I2, (short)_dataID);
                    break;
                case "3":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.I4, (int)_dataID);
                    break;
                case "4":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.U1, (byte)_dataID);
                    break;
                case "5":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.U2, (ushort)_dataID);
                    break;
                case "6":
                    ReplyMSG += _baseExec.GetTypeStringValue(SecsFormat.U4, (uint)_dataID);
                    break;
            }

            //目前暫定所有的CEID為"U4"
            ReplyMSG += "<U4[0] " + sanwaEvent._id.ToString() + ">\r\n";
            ReplyMSG += _baseExec.GetEventReportSML(sanwaEvent, annotated);
            //RPTList end
            ReplyMSG += ">\r\n";


            SecsMessage replyMSG = ReplyMSG.ToSecsMessage();
            //GEM_WBIT_S6
            _ecList.TryGetValue(ECName.GEM_WBIT_S6, out ecObj);
                replyMSG.ReplyExpected = "1" == ecObj._value.ToString() ? true : false;

            _secsGem.SendAsync(replyMSG);
        }
        /// <summary>
        /// 發送Event(不包含Event ID)
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public async Task S6F11Async(string eventName)
        {
            SendEventReportAsync(eventName, false);
        }
        /// <summary>
        /// 發送Event(包含Event ID)
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public async Task S6F13Async(string eventName)
        {
            SendEventReportAsync(eventName, true);
        }
        public async Task S5F1SetAlarmReport(string alarmName, bool set, int alarmMode = 0)
        {
            if (_baseExec == null)      return;
            if (!CheckConnectState())   return;

            _alarmList.TryGetValue(alarmName, out SanwaAlarm sanwaAlarm);

            if (sanwaAlarm == null) return;
            if (sanwaAlarm._enabled == false) return;
            if (sanwaAlarm._set == set) return;
            if (alarmMode > 63) return;

            sanwaAlarm._set = set;

            //暫時不考慮其他設定型態
            Item setItem = null;
            int setvalue;
            setvalue = sanwaAlarm._set ? 0x80 : 0x00;
            setItem = sanwaAlarm._set ? Item.B((byte)(setvalue + alarmMode)) : Item.B((byte)(setvalue + alarmMode));

            //GEM_WBIT_S5
            bool wBIT = false;
            _ecList.TryGetValue(ECName.GEM_WBIT_S5, out SanwaEC ecObj);
            if(ecObj != null)
            {
                if ("1" == ecObj._value.ToString())
                    wBIT = true; 
            }

            SecsMessage s5f1format = _secsMessages[5, 1].FirstOrDefault();
            var s5f1 = new SecsMessage(5, 1, s5f1format.Name,
                    Item.L(setItem,
                    Item.U4(Convert.ToUInt32(sanwaAlarm._id)),
                    Item.A(sanwaAlarm._text)), 
                    wBIT);

            SetDV(DVName.GEM_DV_ALARM_ID, sanwaAlarm._id);

            _secsGem.SendAsync(s5f1);

            _baseExec.SetAlarmSetForSV();
        }
        public async Task S10F1SetTerminalMSG(string terminalText)
        {
            if (_baseExec == null) return;
            if (!CheckConnectState()) return;

            SecsMessage s10f1format = _secsMessages[10, 1].FirstOrDefault();

            //GEM_WBIT_S10
            bool wBIT = false;
            _ecList.TryGetValue(ECName.GEM_WBIT_S10, out SanwaEC ecObj);
            if(ecObj != null)
            {
                if ("1" == ecObj._value.ToString())
                    wBIT = true; 
            }

            var s10f1 = new SecsMessage(10, 1, s10f1format.Name,
                        Item.L(Item.B(0x00),
                        Item.A(terminalText)),
                        wBIT);

            _secsGem.SendAsync(s10f1);

        }
        public async Task<SecsMessage> SetStreamFunction(SecsMessage secsMessage)
        {
            if (_secsGem.State != ConnectionState.Selected)
                return null;
            try
            {
                var reply = await _secsGem.SendAsync(secsMessage);
                return reply;
            }
            catch (Exception ex)
            {
                _logger.Error("SanwaSecs_SetStreamFunction:" + ex.Message);
                return null;
            }

        }
        /// <summary>
        /// SV相關流程:load sv csv文件
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadSVIDCSVFile(string filePath)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string[] raw = line.Split(',');

                    //0.Name
                    //1.ID
                    //2.Format
                    if (!line.Contains("//"))   //第一行不理會
                    {
                        if (_baseExec != null)
                        {
                            SanwaSV temp = new SanwaSV
                            {
                                _name = raw[0],
                                _id = raw[1],
                                _unit = raw[4],
                                _sVName = raw[6]
                            };
                            switch (raw[2].ToUpper())
                            {
                                case "LIST":
                                    temp._format = SecsFormat.List;
                                    Dictionary<string, SanwaSV> svList = new Dictionary<string, SanwaSV>();
                                    temp._value = svList;

                                    break;
                                case "ASCII":   temp._format = SecsFormat.ASCII;    break;
                                case "JIS8":    temp._format = SecsFormat.JIS8; break;
                                case "BINARY":  temp._format = SecsFormat.Binary;   break;
                                case "Boolean": temp._format = SecsFormat.Boolean; break;
                                case "I1": temp._format = SecsFormat.I1; break;
                                case "I2": temp._format = SecsFormat.I2; break;
                                case "I4": temp._format = SecsFormat.I4; break;
                                case "I8": temp._format = SecsFormat.I8; break;
                                case "F8": temp._format = SecsFormat.F8; break;
                                case "F4": temp._format = SecsFormat.F4; break;
                                case "U8": temp._format = SecsFormat.U8; break;
                                case "U1": temp._format = SecsFormat.U1; break;
                                case "U2": temp._format = SecsFormat.U2; break;
                                case "U4": temp._format = SecsFormat.U4; break;
                            }

                            _svList.Add(temp._name, temp);  //Search by name
                            _svIDList.Add(temp._id, temp);  //Search by ID
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("SanwaSecs_LoadSVIDcsvFile:" + ex.Message);
                }

 
            }
            file.Close();
        }
        /// <summary>
        /// SV相關流程:Set SV
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SanwaSV SetSV(string name, object value)
        {
            return _baseExec.SetSV(name, value);
        }
        /// <summary>
        /// SV相關流程:取得SV Object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="SVData"></param>
        public void GetSVData(string name, out SanwaSV SVData)
        {
            _baseExec.GetSVData(name, out SVData);
        }
        /// <summary>
        /// SV相關流程:初始化System SV
        /// </summary>
        /// <param name="name"></param>
        /// <param name="SVData"></param>
        public void SVInitialize()
        {
            SetSV(SVName.GEM_CLOCK, _baseExec.GetDateTime());
            SetSV(SVName.GEM_CONTROL_STATE, _baseExec.GetCurrentStateForSV());
            SetSV(SVName.GEM_CONTROL_STATE, 0);
            SetSV(SVName.GEM_COMM_MODE, 1);
            SetSV(SVName.GEM_PREVIOUS_CEID, 0);

            _ecList.TryGetValue(ECName.GEM_OFF_LINE_SUBSTATE, out SanwaEC ecObj);
            if(ecObj!= null)
                SetSV(SVName.GEM_OFF_LINE_SUB_STATE_SV, ecObj._value);

            SetSV(SVName.GEM_PREVIOUS_CONTROL_STATE, _baseExec.GetCurrentStateForSV());

            _baseExec.SetAlarmEnabledForSV();
            _baseExec.SetAlarmSetForSV();
            _baseExec.SetEventEnabledForSV();

        }
        /// <summary>
        /// EC相關流程:設定EC值
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public SanwaEC SetEV(string name, object value)
        {
            GetEVData(name, out SanwaEC obj);
            if (obj != null)
            {
                SetDV(DVName.GEM_PREVIOUS_EC_VALUE, obj._value);

                switch (obj._type)
                {
                    case SecsFormat.ASCII: obj._value = value.ToString(); break;
                    case SecsFormat.Boolean: obj._value = Convert.ToBoolean(value); break;
                    case SecsFormat.Binary:
                        var enumerable = value as IEnumerable<byte>;
                        obj._value = enumerable;
                        break;
                    case SecsFormat.F4:
                        if(!(Convert.ToSingle(value) - Convert.ToSingle(obj._maxValue) > 0.0 ||
                            Convert.ToSingle(value) - Convert.ToSingle(obj._minValue) < 0.0))
                            obj._value = Convert.ToSingle(value);
                        break;
                    case SecsFormat.F8:
                        if (!(Convert.ToDouble(value) - Convert.ToDouble(obj._maxValue) > 0.0 ||
                            Convert.ToDouble(value) - Convert.ToDouble(obj._minValue) < 0.0))
                            obj._value = Convert.ToDouble(value);
                        break;
                    case SecsFormat.I1:
                        if (!(Convert.ToSByte(value) > Convert.ToSByte(obj._maxValue) ||
                            Convert.ToSByte(value) < Convert.ToSByte(obj._minValue) ))
                            obj._value = Convert.ToSByte(value);
                        break;
                    case SecsFormat.I2:
                        if (!(Convert.ToInt16(value) > Convert.ToInt16(obj._maxValue) ||
                            Convert.ToInt16(value) < Convert.ToInt16(obj._minValue)))
                            obj._value = Convert.ToInt16(value);
                        break;
                    case SecsFormat.I4:
                        if (!(Convert.ToInt32(value) > Convert.ToInt32(obj._maxValue) ||
                            Convert.ToInt32(value) < Convert.ToInt32(obj._minValue)))
                            obj._value = Convert.ToInt32(value);
                        break;
                    case SecsFormat.I8:
                        if (!(Convert.ToInt64(value) > Convert.ToInt64(obj._maxValue) ||
                            Convert.ToInt64(value) < Convert.ToInt64(obj._minValue)))
                            obj._value = Convert.ToInt64(value);
                        break;
                    case SecsFormat.JIS8: obj._value = value.ToString(); break;
                    case SecsFormat.U1:
                        if (!(Convert.ToByte(value) > Convert.ToByte(obj._maxValue) ||
                            Convert.ToByte(value) < Convert.ToByte(obj._minValue)))
                            obj._value = Convert.ToByte(value);
                        break;
                    case SecsFormat.U2:
                        if (!(Convert.ToUInt16(value) > Convert.ToUInt16(obj._maxValue) ||
                            Convert.ToUInt16(value) < Convert.ToUInt16(obj._minValue)))
                            obj._value = Convert.ToUInt16(value);
                        break;
                    case SecsFormat.U4:
                        if (!(Convert.ToUInt32(value) > Convert.ToUInt32(obj._maxValue) ||
                            Convert.ToUInt32(value) < Convert.ToUInt32(obj._minValue)))
                            obj._value = Convert.ToUInt32(value);
                        break;
                    case SecsFormat.U8:
                        if (!(Convert.ToUInt64(value) > Convert.ToUInt64(obj._maxValue) ||
                            Convert.ToUInt64(value) < Convert.ToUInt64(obj._minValue)))
                            obj._value = Convert.ToUInt64(value);
                        break;
                }

                //if(obj._value == value)
                S6F11Async(EventName.GEM_EQ_CONST_CHANGED);

                SetDV(DVName.GEM_DV_ECID_CHANGED, obj._id);
                SetDV(DVName.GEM_DV_EC_VALUE_CHANGED, obj._value);
            }

            return obj;
        }
        /// <summary>
        /// EC相關流程:取得EC Object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="SVData"></param>
        public void GetEVData(string name, out SanwaEC SVData)
        {
            if (_baseExec != null)
            {
                _ecList.TryGetValue(name, out SanwaEC obj);
                SVData = obj;
            }
            else
            {
                SVData = null;
            }
        }
        private void LoadECCSVFile(string filePath)
        {
            string line;
            bool readfilerror = false;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string[] raw = line.Split(',');


                    //[0]_nameDefine;
                    //[1]_id;
                    //[2]_type;
                    //[3]_minValue;
                    //[4]_maxValue;
                    //[5]_defaultValue;
                    //[6]_unit;
                    //[7]_name;
                    //[8]_remark;
                    if (!line.Contains("//"))
                    {
                        if (_baseExec != null)
                        {
                            SanwaEC temp = new SanwaEC
                            {
                                _name = raw[0],
                                _id = raw[1],
                                _unit = raw[6],
                                _comment = raw[7],

                                _minValue = null,
                                _maxValue = null,
                                _defaultValue = null
                                 
                            };

                            switch (raw[2].ToUpper())
                            {
                                case "ASCII":
                                    temp._type = SecsFormat.ASCII;
                                    temp._minValue = raw[3];
                                    temp._maxValue = raw[4];
                                    temp._value = temp._defaultValue = raw[5];
                                    break;
                                case "B":
                                    temp._type = SecsFormat.Binary;

                                    string[] vector3 = raw[3].Split(' ');
                                    string[] vector4 = raw[4].Split(' ');
                                    string[] vector5 = raw[5].Split(' ');

                                    if (vector3.Length == vector4.Length &&
                                        vector3.Length == vector5.Length)
                                    {
                                        if (raw[3] != "")
                                        {
                                            byte[] minvalueVector = new byte[vector3.Length];
                                            for(int i = 0; i< minvalueVector.Length; i++)
                                            {
                                                minvalueVector[i] = Convert.ToByte(vector3[i]);
                                            }
                                            temp._value = temp._minValue = minvalueVector;

                                        }

                                        if(raw[4] != "")
                                        {
                                            byte[] maxvalueVector = new byte[vector4.Length];
                                            for (int i = 0; i < maxvalueVector.Length; i++)
                                            {
                                                maxvalueVector[i] = Convert.ToByte(vector4[i]);
                                            }
                                            temp._value = temp._maxValue = maxvalueVector;

                                        }

                                        if (raw[5] != "")
                                        {
                                            byte[] defaultvalueVector = new byte[vector5.Length];
                                            for (int i = 0; i < defaultvalueVector.Length; i++)
                                            {
                                                defaultvalueVector[i] = Convert.ToByte(vector5[i]);
                                            }
                                            temp._value = temp._defaultValue = defaultvalueVector;
                                        }
                                    }                                    
                                    break;

                                case "BOOLEAN": temp._type = SecsFormat.Boolean;
                                    temp._minValue = raw[3] == "0" ? false : true;
                                    temp._maxValue = raw[4] == "0" ? false : true;
                                    temp._value = temp._defaultValue = raw[5] == "0" ? false : true;
                                    break;

                                case "I1": temp._type = SecsFormat.I1;
                                    if(raw[3] != "")
                                        temp._minValue = Convert.ToSByte(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToSByte(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToSByte(raw[5]);
                                    break;

                                case "I2": temp._type = SecsFormat.I2;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToInt16(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToInt16(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToInt16(raw[5]);
                                    break;

                                case "I4": temp._type = SecsFormat.I4;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToInt32(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToInt32(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToInt32(raw[5]);

                                    break;

                                case "I8": temp._type = SecsFormat.I8;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToInt64(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToInt64(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToInt64(raw[5]);
                                    break;

                                case "F8": temp._type = SecsFormat.F8;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToDouble(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToDouble(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToDouble(raw[5]);                                        
                                    break;

                                case "F4": temp._type = SecsFormat.F4;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToSingle(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToSingle(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToSingle(raw[5]);
                                    break;

                                case "U8": temp._type = SecsFormat.U8;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToUInt64(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToUInt64(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToUInt64(raw[5]);
                                    break;

                                case "U1": temp._type = SecsFormat.U1;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToByte(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToByte(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToByte(raw[5]);
                                    break;

                                case "U2": temp._type = SecsFormat.U2;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToUInt16(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToUInt16(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToUInt16(raw[5]);
                                    break;

                                case "U4": temp._type = SecsFormat.U4;
                                    if (raw[3] != "")
                                        temp._minValue = Convert.ToUInt32(raw[3]);
                                    if (raw[4] != "")
                                        temp._maxValue = Convert.ToUInt32(raw[4]);
                                    if (raw[5] != "")
                                        temp._value = temp._defaultValue = Convert.ToUInt32(raw[5]);
                                    break;

                                default:
                                    readfilerror = true;
                                    break;
                            }



                            _ecList.Add(temp._name, temp);
                            _ecIDList.Add(temp._id, temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("SanwaSecs_LoadECCSVFile:" + ex.Message);
                }

                if (readfilerror)
                {
                    _logger.Error("SanwaSecs_LoadECCSVFile:Format Error");
                }
            }

            file.Close();
        }
        /// <summary>
        /// Event 相關流程
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadEventIDCSVFile(string filePath)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string[] raw = line.Split(',');

                    //[0] _nameDefine;
                    //[1] _id;
                    //[2] _name;
                    if (!line.Contains("//"))
                    {
                        if (_baseExec != null)
                        {
                            SanwaEvent temp = new SanwaEvent
                            {
                                _name = raw[0],
                                _id = raw[1],
                                _comment = raw[2]
                            };
                            _eventList.Add(temp._name, temp);
                            _eventIDList.Add(temp._id, temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("SanwaSecs_LoadEventIDCSVFile:" + ex.Message);
                }
            }

            file.Close();
        }
        /// <summary>
        /// Alarm 相關流程:load Alarm csv檔
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadAlarmCSVFile(string filePath)
        {
            string line;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string[] raw = line.Split(',');
                    //[0]_name;
                    //[1]_id;
                    //[2]_cd;
                    //[3]_enabled;
                    //[4]_text;
                    if (!line.Contains("//"))
                    {
                        if (_baseExec != null)
                        {
                            SanwaAlarm temp = new SanwaAlarm()
                            {
                                _name = raw[0],
                                _id = raw[1],
                                _cd = raw[2],
                                _text = raw[4],

                                _enabled = raw[3] == "1" ? true : false
                            };

                            _alarmList.Add(temp._name, temp);
                            _alarmIDList.Add(temp._id, temp);
                        }                        
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("LoadAlarmCSVFile:" + ex.Message);
                }
            }

            file.Close();
        }
        /// <summary>
        /// DV相關流程:設定DV
        /// </summary>
        /// <param name="name"></param>
        /// <param name="DVData"></param>
        public SanwaDV SetDV(string name, object value)
        {
            GetDVData(name, out SanwaDV obj);

            if (obj != null)
            {
                switch (obj._format)
                {
                    case SecsFormat.ASCII: obj._value = value.ToString(); break;
                    case SecsFormat.Binary:
                        var enumerable = value as IEnumerable<byte>;
                        obj._value = enumerable;
                        break;
                    case SecsFormat.Boolean: obj._value = Convert.ToBoolean(value); break;
                    case SecsFormat.F4: obj._value = Convert.ToSingle(value); break;
                    case SecsFormat.F8: obj._value = Convert.ToDouble(value); break;
                    case SecsFormat.I1: obj._value = Convert.ToSByte(value); break;
                    case SecsFormat.I2: obj._value = Convert.ToInt16(value); break;
                    case SecsFormat.I4: obj._value = Convert.ToInt32(value); break;
                    case SecsFormat.I8: obj._value = Convert.ToInt64(value); break;
                    case SecsFormat.JIS8: obj._value = value.ToString(); break;
                    case SecsFormat.U1: obj._value = Convert.ToByte(value); break;
                    case SecsFormat.U2: obj._value = Convert.ToUInt16(value); break;
                    case SecsFormat.U4: obj._value = Convert.ToUInt32(value); break;
                    case SecsFormat.U8: obj._value = Convert.ToUInt64(value); break;
                }
            }

            return obj;
        }
        /// <summary>
        /// DV流程:取得DV Object
        /// </summary>
        /// <param name="name"></param>
        /// <param name="DVData"></param>
        public void GetDVData(string name, out SanwaDV DVData)
        {
            if (_baseExec != null)
            {
                _dvList.TryGetValue(name, out SanwaDV obj);
                DVData = obj;
            }
            else
            {
                DVData = null;
            }
        }
        /// <summary>
        /// DV流程:讀取DV csv檔
        /// </summary>
        /// <param name="filePath"></param>
        private void LoadDVCSVFile(string filePath)
        {
            string line;
            bool readfilerror = false;
            System.IO.StreamReader file = new System.IO.StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                try
                {
                    string[] raw = line.Split(',');

                    if (!line.Contains("//"))
                    {
                        if (_baseExec != null)
                        {
                            SanwaDV temp = new SanwaDV
                            {
                                //[0]_name	
                                //[1]_id	
                                //[2]_type	
                                //[3]_length	
                                //[4]_unit	
                                //[5]_definition
                                _name = raw[0],
                                _id = raw[1],
                                _length = raw[3],
                                _unit = raw[4],
                                _definition = raw[5]
                            };

                            switch (raw[2].ToUpper())
                            {
                                case "LIST":
                                    temp._format = SecsFormat.List;
                                    Dictionary<string, SanwaDV> dvList = new Dictionary<string, SanwaDV>();
                                    temp._value = dvList;

                                    break;
                                case "ASCII": temp._format = SecsFormat.ASCII; break;
                                case "JIS8": temp._format = SecsFormat.JIS8; break;
                                case "BINARY": temp._format = SecsFormat.Binary; break;
                                case "Boolean": temp._format = SecsFormat.Boolean; break;
                                case "I1": temp._format = SecsFormat.I1; break;
                                case "I2": temp._format = SecsFormat.I2; break;
                                case "I4": temp._format = SecsFormat.I4; break;
                                case "I8": temp._format = SecsFormat.I8; break;
                                case "F8": temp._format = SecsFormat.F8; break;
                                case "F4": temp._format = SecsFormat.F4; break;
                                case "U8": temp._format = SecsFormat.U8; break;
                                case "U1": temp._format = SecsFormat.U1; break;
                                case "U2": temp._format = SecsFormat.U2; break;
                                case "U4": temp._format = SecsFormat.U4; break;
                            }

                            _dvList.Add(temp._name, temp);
                            _dvIDList.Add(temp._id, temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("SanwaSecs_LoadECCSVFile:" + ex.Message);
                }

                if (readfilerror)
                {
                    _logger.Error("SanwaSecs_LoadECCSVFile:Format Error");
                }
            }

            file.Close();
        }
        public bool CheckFomart20(Item item)
        {
            return _baseExec.CheckFomart20(item);
        }
        public bool CheckFomart3x(Item item)
        {
            return _baseExec.CheckFomart3x(item);
        }
        public bool CheckFomart5x(Item item)
        {
            return _baseExec.CheckFomart5x(item);
        }
        public bool CheckFomart3x5x(Item item)
        {
            return _baseExec.CheckFomart3x5x(item);
        }
        public bool CheckFomart3x5x20(Item item)
        {
            return _baseExec.CheckFomart3x5x20(item);
        }
        public string GetSMLName(int s, int f)
        {
            string strRet = "";
            try
            {

                SecsMessage secsMSG = _secsMessages[(byte)s, (byte)f].FirstOrDefault();
                strRet = _baseExec.GetMessageName(secsMSG.ToSml());
            }
            catch (Exception e)
            {
                _logger.Error("SanwaSecs_GetSMLName:" + e.Message);
            }

            return strRet;
        }
        public void ReplyUnrecognizedStreamType(PrimaryMessageWrapper e)
        {
            _baseExec.ReplyUnrecognizedStreamType(e);
        }
        public void ReplyUnrecognizedFunctionType(PrimaryMessageWrapper e)
        {
            _baseExec.ReplyUnrecognizedFunctionType(e);
        }
        public void ReplyIllegalData(PrimaryMessageWrapper e)
        {
            _baseExec.ReplyIllegalData(e);
        }

    }
}