//using System.Collections.Generic;

//public class Burn : TaskBase
//{
//    public Burn()
//    {
//    }

//    public Dictionary<ManaColor, int> ManaToBurn;
//    public int AmountToChannel;

//    public Burn(Dictionary<ManaColor, int> castKvp)
//    {
//        ManaToBurn = castKvp;
//    }

//    public override bool Done()
//    {
//        if (Faction.QueueComplete(SubTasks))
//        {
//            foreach (var kvp in ManaToBurn)
//            {
//                if (kvp.Value > 0)
//                {
//                    AddSubTask(new Wait(1, $"{kvp.Key}!!", true));
//                    ManaToBurn[kvp.Key]--;
//                    Creature.BurnMana(kvp.Key);

//                    return false;
//                }
//            }
//            return true;
//        }

//        return false;
//    }
//}