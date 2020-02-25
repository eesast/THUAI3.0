﻿using Communication.Proto;
using Logic.Constant;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;

namespace Logic.Server
{
    public class Trigger : Obj
    {
        public TriggerType triggerType;
        public int OwnerTeam;
        public System.Threading.Timer DurationTimer;
        public Trigger(double x_t, double y_t, TriggerType type_t, int owner_t) : base(x_t, y_t, ObjType.Trigger)
        {
            Layer = (int)MapLayer.TriggerLayer;
            Movable = false;
            triggerType = type_t;
            OwnerTeam = owner_t;
            if (triggerType == TriggerType.WaveGlue) DurationTimer = new System.Threading.Timer(
                (i) => { Parent = null; }, null, (int)(Configs["WaveGlueDuration"]), 0);
            this.OnTrigger += new TriggerHandler(
                (t) =>
                {
                    if (triggerType != TriggerType.WaveGlue)
                        this.Parent = null;
                });
            AddToMessage();
            lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectMessageList[ID].TriggerType = (TriggerTypeMessage)triggerType;
            this.OnParentDelete += new ParentDeleteHandler(DeleteFromMessage);

        }
    }
}
