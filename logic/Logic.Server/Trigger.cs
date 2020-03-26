using Communication.Proto;
using Logic.Constant;
using static Logic.Constant.Constant;
using static Logic.Constant.MapInfo;

namespace Logic.Server
{
    public class Trigger : Obj
    {
        public TriggerType triggerType;
        public int OwnerTeam;
        public int hitScore = 0;
        public int stunTime = 0;
        public System.Threading.Timer DurationTimer;
        public Trigger(double x_t, double y_t, TriggerType type_t, int ownerTeam, Talent ownerTalent) : base(x_t, y_t, ObjType.Trigger)
        {
            Layer = TriggerLayer;
            Movable = false;
            triggerType = type_t;
            OwnerTeam = ownerTeam;
            switch (triggerType)
            {
                case TriggerType.Mine:
                    hitScore = (int)((ownerTalent == Talent.Technician) ? Configs["Talent"]["Technician"]["Mine"]["Score"] : Configs["Trigger"]["Mine"]["Score"]);
                    stunTime = (int)((ownerTalent == Talent.Technician) ? Configs["Talent"]["Technician"]["Mine"]["StunDuration"] : Configs["Trigger"]["Mine"]["StunDuration"]);
                    break;
                case TriggerType.Trap:
                    stunTime = (int)((ownerTalent == Talent.Technician) ? Configs["Talent"]["Technician"]["Trap"]["StunDuration"] : Configs["Trigger"]["Trap"]["StunDuration"]);
                    break;
                case TriggerType.WaveGlue:
                    DurationTimer = new System.Threading.Timer((i) => { Parent = null; }, null, (int)Configs["Trigger"]["WaveGlue"]["Duration"], 0);
                    break;
                case TriggerType.Bomb:
                    stunTime = (int)((ownerTalent == Talent.Technician) ? Configs["Talent"]["Technician"]["Bomb"]["StunDuration"] : Configs["Trigger"]["Bomb"]["StunDuration"]);
                    break;
                case TriggerType.Arrow:
                    Movable = true;
                    hitScore = (int)((ownerTalent == Talent.StrongMan) ? Configs["Talent"]["StrongMan"]["Arrow"]["Score"] : Configs["Trigger"]["Arrow"]["Score"]);
                    stunTime = (int)((ownerTalent == Talent.StrongMan) ? Configs["Talent"]["StrongMan"]["Arrow"]["StunDuration"] : Configs["Trigger"]["Arrow"]["StunDuration"]);
                    break;
                case TriggerType.Hammer:
                    Movable = true;
                    stunTime = (int)((ownerTalent == Talent.StrongMan) ? Configs["Talent"]["StrongMan"]["Hammer"]["StunDuration"] : Configs["Trigger"]["Hammer"]["StunDuration"]);
                    break;
            }
            this.StopMoving += new StopMovingHandler(o => { Parent = null; });
            AddToMessage();
            lock (Program.MessageToClientLock)
                Program.MessageToClient.GameObjectList[ID].TriggerType = triggerType;
            this.MoveComplete += new MoveCompleteHandler(ChangePositionInMessage);
            this.OnParentDelete += new ParentDeleteHandler(DeleteFromMessage);
            Server.ServerDebug("Create trigger : " + triggerType.ToString());
        }
    }

}
