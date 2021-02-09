using DuckGame;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
#pragma warning disable CS1591

namespace Unlock_Modifies
{
    public partial class UnlockModifiersMod : DisabledMod, IUpdateable
    {
        /// <summary>
        /// <para>Change the value in OnPreInitialize</para>
        /// <para>If you want</para>
        /// </summary>
        public static bool AllowToModifiyTicketCount = true;

        private byte wait;

        #region IUpdateable Implementations
        public bool Enabled => true;

        public int UpdateOrder => 1;

        public event EventHandler<EventArgs> EnabledChanged;

        public event EventHandler<EventArgs> UpdateOrderChanged;
        #endregion

        protected override void OnPostInitialize()
        {
            (typeof(Game).GetField("updateableComponents", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(MonoMain.instance) as List<IUpdateable>).Add(this);
        }

        public void Update(GameTime gameTime)
        {
            if (++wait is < 60) return;

            DoUnlockModifies();
            wait = 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void FixTicketCount()
        {
            foreach (Profile profile in Profiles.core.all)
            {
                profile.ticketCount = int.MaxValue;
            }
        }

        public static void DoUnlockModifies()
        {
            foreach (Profile profile in Profiles.core.all)
            {
                bool allModifiesUnlocked = true;
                #region Do unlock all modifies
                foreach (UnlockData unlock in Unlocks.allUnlocks)
                {
                    if (unlock.unlocked) continue;
                    if (unlock.type == UnlockType.Modifier)
                    {
                        profile.unlocks.Add(unlock.id);
                        allModifiesUnlocked = false;
                        #region Fix ticket count for new users
                        if (AllowToModifiyTicketCount)
                        {
                            profile.ticketCount += unlock.cost;
                        }
                        #endregion
                    }
                }
                #endregion
                #region Try fix old users ticket count
                if (allModifiesUnlocked && profile.ticketCount < 0)
                {
                    foreach (UnlockData unlock in Unlocks.allUnlocks)
                    {
                        if (unlock.type == UnlockType.Modifier && unlock.unlocked)
                        {
                            profile.ticketCount += unlock.cost;
                        }
                    }
                    if (profile.ticketCount < 0) profile.ticketCount = 0;
                }
                #endregion
            }
        }
    }
}
