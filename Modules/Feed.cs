using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using KushBot.Data;
using KushBot.DataClasses;

namespace KushBot.Modules
{
    public class Feed : ModuleBase<SocketCommandContext>
    {
        [Command("Feed")]
        public async Task Level([Remainder]string _PetIndex)
        {
            int PetIndex = 0;
            //_PetIndex == "Jew" || _PetIndex == 0.ToString() || _PetIndex == "jew"
            if (_PetIndex.Equals(Program.Pets[0].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex == 0.ToString() || _PetIndex.Equals("superned", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 0;
                //_PetIndex == "Pinata" || _PetIndex == 1.ToString() || _PetIndex == "pinata" || _PetIndex == "Baps Pinata"
            }
            else if (_PetIndex == 1.ToString() || _PetIndex.Equals("Pinata", StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("Baps Pinata", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 1;
            }else if (_PetIndex == 2.ToString() || _PetIndex.Equals(Program.Pets[2].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("goran", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 2;
            }
            else if (_PetIndex == 3.ToString() || _PetIndex.Equals(Program.Pets[3].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("gambeat", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 3;
            }
            else if (_PetIndex == 4.ToString() || _PetIndex.Equals(Program.Pets[4].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("jew", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 4;
            }else if(_PetIndex == 5.ToString() || _PetIndex.Equals(Program.Pets[5].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("tylerjuan", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 5;
            }
            else if (_PetIndex == 6.ToString() || _PetIndex.Equals(Program.Pets[6].Name, StringComparison.CurrentCultureIgnoreCase) || _PetIndex.Equals("Maybich", StringComparison.CurrentCultureIgnoreCase))
            {
                PetIndex = 6;
            }
            //else if (_PetIndex == 2.ToString() || _PetIndex.Equals(Program.Pets[2].Name, StringComparison.CurrentCultureIgnoreCase) ||
            //    _PetIndex.Equals("goran", StringComparison.CurrentCultureIgnoreCase) ||
            //    _PetIndex.Equals("jelic", StringComparison.CurrentCultureIgnoreCase))
            //{
            //    PetIndex = 2;
            //}
            else
            {
                await ReplyAsync($"{Context.User.Mention} No such pet exists, are you fucking dyslexic or some shit?");
                return;
            }

            if (!Exists(Data.Data.GetPets(Context.User.Id), PetIndex))
            {
                await ReplyAsync($"{Context.User.Mention}, you don't have the {Program.Pets[PetIndex].Name} pet, Dumb fuck...");
                return;
            }

            if (PetIndex < 0 || PetIndex > Program.Pets.Count)
            {
                await ReplyAsync($"{Context.User.Mention} Good luck working in McDonalds with that math.");
                return;
            }

            if (Data.Data.GetPetLevel(Context.User.Id, PetIndex) - Data.Data.GetItemPetLevel(Context.User.Id, PetIndex) == 99)
            {
                await ReplyAsync($"{Context.User.Mention} Your pet is already level 99 <:gana:945781528699474053>");
                return;
            }

            int petLevel = Data.Data.GetPetLevel(Context.User.Id, PetIndex);
            int itemPetLevel = Data.Data.GetItemPetLevel(Context.User.Id, PetIndex);

            double negate = 0;
            if (petLevel - itemPetLevel < 15)
            {
                negate = (double)(petLevel - itemPetLevel) / 100;
            }
            else
            {
                negate = 0.14;
            }

            int BapsFed = 0;

            if (petLevel - itemPetLevel == 1)
            {
                BapsFed = 100;
            }

            else
            {
                double _BapsFed = Math.Pow(petLevel - itemPetLevel, 1.14 - negate) * (70 + ((petLevel - itemPetLevel) / 1.25));
                BapsFed = (int)Math.Round(_BapsFed);
            }

            if(BapsFed > Data.Data.GetBalance(Context.User.Id))
            {
                await ReplyAsync($"{Context.User.Mention} Can't even buy proper food for his pet, fucking loser");
                return;
            }

            await Data.Data.SavePetLevels(Context.User.Id,PetIndex,petLevel - itemPetLevel + 1, false);
            await TutorialManager.AttemptSubmitStepCompleteAsync(Context.User.Id, 4, 0, Context.Channel);
            await ReplyAsync($"{Context.User.Mention} You have fed your **{Program.Pets[PetIndex].Name}** {BapsFed} baps and it's now level **{Data.Data.GetPetLevel(Context.User.Id,PetIndex)}**");

            await Data.Data.SaveBalance(Context.User.Id,BapsFed * -1, false);

            List<int> QuestIndexes = new List<int>();
            #region Assignment
            string hold = Data.Data.GetQuestIndexes(Context.User.Id);
            string[] values = hold.Split(',');

            for (int i = 0; i < values.Length; i++)
            {
                QuestIndexes.Add(int.Parse(values[i]));
            }
            #endregion


            if (QuestIndexes.Contains(13))
            {
                await Program.CompleteQuest(13, QuestIndexes, Context.Channel, Context.User);
            }

        }
        public bool Exists(string text, int match)
        {
            for (int i = 0; i < text.Length; i++)
            {
                int temp = int.Parse(text[i].ToString());
                if (temp == match)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
