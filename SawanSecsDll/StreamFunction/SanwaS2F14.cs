﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.IO;

namespace SawanSecsDll
{
    public partial class SanwaBaseExec
    {
        public void ReplyS2F14(PrimaryMessageWrapper e, SecsMessage receiveMsg, SecsMessage replyMsg)
        {
            //儲存所有要顯示的EC
            List<string> ecidlist = new List<string>();

            CheckReceiveIDList(receiveMsg, ref ecidlist, _ecList);

            string newReplyMsg = GetMessageName(replyMsg.ToSml());

            newReplyMsg += "< L[" + ecidlist.Count.ToString() + "]\r\n";

            newReplyMsg = RecursivelyECList(_ecList, ecidlist, newReplyMsg);

            newReplyMsg += ">";

            e.ReplyAsync(newReplyMsg.ToSecsMessage());
        }
        private string RecursivelyECList(Dictionary<string, SanwaEC> eCList, List<string> eCIDList, string ReplyMSG)
        {
            foreach (string iD in eCIDList)
            {
                eCList.TryGetValue(iD, out SanwaEC Obj);

                if (Obj == null)
                {
                    ReplyMSG += "<L[0]\r\n>\r\n";
                    continue;
                }

                if (Obj._defaultValue == null)
                {
                    ReplyMSG += "<L[0]\r\n>\r\n";
                    continue;
                }

                switch (Obj._type)
                {
                    case SecsFormat.List:
                        Dictionary<string, SanwaEC> _ecSubList = (Dictionary<string, SanwaEC>)Obj._defaultValue;
                        List<string> ECList = new List<string>();
                        foreach (var subObj in _ecSubList) ECList.Add(subObj.Key);
                        ReplyMSG = ReplyMSG + "<L [" + ECList.Count.ToString() + "]\r\n";
                        ReplyMSG = RecursivelyECList(_ecSubList, ECList, ReplyMSG);
                        ReplyMSG += ">\r\n";
                        break;

                    default:
                        ReplyMSG += GetTypeStringValue(Obj._type, Obj._value);
                        break;
                }
            }

            return ReplyMSG;
        }
    }
}
