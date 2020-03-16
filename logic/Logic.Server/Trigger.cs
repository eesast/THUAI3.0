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
        public int parameter = 0;
        public System.Threading.Timer DurationTimer;
        public Trigger(double x_t, double y_t, TriggerType type_t, int owner_t, bool tech = false) : base(x_t, y_t, ObjType.Trigger)
        {
            Layer = TriggerLayer;
            Movable = false;
            triggerType = type_t;
            OwnerTeam = owner_t;
            switch (triggerType)
            {
                case TriggerType.Mine:
                    parameter = (int)(tech ? Configs["TechnicianMineScore"] : Configs["MineScore"]);
                    break;
                case TriggerType.Trap:
                    parameter = (int)(tech ? Configs["TechnicianTrapStunDuration"] : Configs["TrapStunDuration"]);
                    break;
                case TriggerType.WaveGlue:
                    DurationTimer = new System.Threading.Timer((i) => { Parent = null; }, null, (int)Configs["WaveGlueDuration"], 0);
                    break;
            }
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
