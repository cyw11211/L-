﻿using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace xSaliceReligionAIO.Champions
{
    class Annie : Champion
    {
        public Annie()
        {
            LoadSpell();
            LoadMenu();
        }

        private void LoadSpell()
        {
            Q = new Spell(SpellSlot.Q, 625);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E);
            R = new Spell(SpellSlot.R, 600);
            R2 = new Spell(SpellSlot.R, 1000);

            Q.SetTargetted(.25f, 1400f);
            W.SetSkillshot(0.25f, 80, float.MaxValue, false, SkillshotType.SkillshotCone);
            R.SetSkillshot(0.25f, 225f, float.MaxValue, false, SkillshotType.SkillshotCircle);
            R2.SetSkillshot(0.35f, 225f, float.MaxValue, false, SkillshotType.SkillshotCircle);
        }

        private void LoadMenu()
        {
            var key = new Menu("按键", "Key");
            {
                key.AddItem(new MenuItem("ComboActive", "连招", true).SetValue(new KeyBind(32, KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActive", "消耗", true).SetValue(new KeyBind("C".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("HarassActiveT", "消耗(锁定)", true).SetValue(new KeyBind("N".ToCharArray()[0], KeyBindType.Toggle)));
                key.AddItem(new MenuItem("LaneClearActive", "清兵", true).SetValue(new KeyBind("V".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQ", "Q补兵", true).SetValue(new KeyBind("A".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("LastHitQToggle", "Q补兵(锁定)", true).SetValue(new KeyBind("J".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("FlashKILL", "闪现击杀", true).SetValue(new KeyBind("T".ToCharArray()[0], KeyBindType.Press)));
                key.AddItem(new MenuItem("ChargeStun", "叠加被动(锁定)", true).SetValue(new KeyBind("I".ToCharArray()[0], KeyBindType.Toggle)));
                //add to menu
                menu.AddSubMenu(key);
            }

            var spellMenu = new Menu("技能", "SpellMenu");
            {
                var qMenu = new Menu("Q菜单", "QMenu");
                {
                    qMenu.AddItem(new MenuItem("Q_Stun_LastHit", "有被动也Q补兵", true).SetValue(false));
                    spellMenu.AddSubMenu(qMenu);
                }

                var wMenu = new Menu("W菜单", "WMenu");
                {
                    wMenu.AddItem(new MenuItem("useW_enemyCount", "Auto W If Stun >=", true).SetValue(new Slider(3, 1, 5)));
                    wMenu.AddItem(new MenuItem("W_charge", "Use W to charge stun", true).SetValue(true));
                    spellMenu.AddSubMenu(wMenu);
                }

                var eMenu = new Menu("E菜单", "EMenu");
                {
                    eMenu.AddItem(new MenuItem("E_Charge", "E叠加被动", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Block", "被攻击自动E", true).SetValue(true));
                    eMenu.AddItem(new MenuItem("E_Stun", "有被动不E", true).SetValue(true));
                    spellMenu.AddSubMenu(eMenu);
                }

                var rMenu = new Menu("R菜单", "RMenu");
                {
                    rMenu.AddItem(new MenuItem("useR_enemyCount", "自动大晕人数 >=, 6 = 关闭", true).SetValue(new Slider(3, 1, 6)));
                    rMenu.AddItem(new MenuItem("R_To_KS", "R击杀", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("R_OverKill", "击杀检测", true).SetValue(true));
                    rMenu.AddItem(new MenuItem("R_Mode", "管理小熊模式", true).SetValue(new StringList(new[] { "跟随目标", "跟随我" })));
                    spellMenu.AddSubMenu(rMenu);
                }
                //add to menu
                menu.AddSubMenu(spellMenu);
            }

            var combo = new Menu("连招", "Combo");
            {
                combo.AddItem(new MenuItem("UseQCombo", "使用Q", true).SetValue(true));
                combo.AddItem(new MenuItem("UseWCombo", "使用W", true).SetValue(true));
                combo.AddItem(new MenuItem("UseECombo", "使用E", true).SetValue(true));
                combo.AddItem(new MenuItem("UseRCombo", "使用R", true).SetValue(true));
                combo.AddItem(new MenuItem("R_Killable", "只能击杀用R", true).SetValue(new KeyBind("H".ToCharArray()[0], KeyBindType.Toggle)));
                combo.AddItem(new MenuItem("useR_enemyCount_Combo", "R If Stun >=", true).SetValue(new Slider(3, 1, 5)));
                combo.AddItem(new MenuItem("FlashStun", "使用闪现->R->眩晕击杀", true).SetValue(true));
                //add to menu
                menu.AddSubMenu(combo);
            }
            var harass = new Menu("消耗", "Harass");
            {
                harass.AddItem(new MenuItem("UseQHarass", "使用Q", true).SetValue(true));
                harass.AddItem(new MenuItem("UseWHarass", "使用W", true).SetValue(true));
                harass.AddItem(new MenuItem("UseEHarass", "使用E", true).SetValue(true));
                harass.AddItem(new MenuItem("Harass_Tower", "塔下消耗", true).SetValue(true));
                AddManaManagertoMenu(harass, "Harass", 30);
                //add to menu
                menu.AddSubMenu(harass);
            }
            var farm = new Menu("清线", "LaneClear");
            {
                farm.AddItem(new MenuItem("UseQFarm", "使用Q", true).SetValue(true));
                farm.AddItem(new MenuItem("UseWFarm", "使用W", true).SetValue(true));
                farm.AddItem(new MenuItem("LaneClear_useW_minHit", "最小W小兵数", true).SetValue(new Slider(2, 1, 6)));
                //add to menu
                menu.AddSubMenu(farm);
            }

            var misc = new Menu("杂项", "Misc");
            {
                misc.AddItem(new MenuItem("Interrupt", "用眩晕打断击杀", true).SetValue(true));
                misc.AddItem(new MenuItem("W_Gap", "用眩晕防突进", true).SetValue(true));
                misc.AddItem(new MenuItem("smartKS", "智能击杀", true).SetValue(true));
                misc.AddItem(new MenuItem("chargeMana", "叠加被动法力 >=", true).SetValue(new Slider(30)));
                misc.AddItem(new MenuItem("chargeInFountain", "在泉水叠加被动", true).SetValue(true));
                misc.AddItem(new MenuItem("disableAA", "技能没好不普攻", true).SetValue(false));
                menu.AddSubMenu(misc);
            }

            var drawMenu = new Menu("显示", "Drawing");
            {
                drawMenu.AddItem(new MenuItem("Draw_Disabled", "关闭所有", true).SetValue(false));
                drawMenu.AddItem(new MenuItem("Draw_Q", "Q范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_W", "W范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R", "R范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("Draw_R2", "闪现大范围", true).SetValue(true));
                drawMenu.AddItem(new MenuItem("FlashStunTarget", "显示闪现大准备", true).SetValue(true));

                MenuItem drawComboDamageMenu = new MenuItem("Draw_ComboDamage", "显示连招伤害", true).SetValue(true);
                MenuItem drawFill = new MenuItem("Draw_Fill", "显示连招完整伤害", true).SetValue(new Circle(true, Color.FromArgb(90, 255, 169, 4)));
                drawMenu.AddItem(drawComboDamageMenu);
                drawMenu.AddItem(drawFill);
                DamageIndicator.DamageToUnit = GetComboDamage;
                DamageIndicator.Enabled = drawComboDamageMenu.GetValue<bool>();
                DamageIndicator.Fill = drawFill.GetValue<Circle>().Active;
                DamageIndicator.FillColor = drawFill.GetValue<Circle>().Color;
                drawComboDamageMenu.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Enabled = eventArgs.GetNewValue<bool>();
                    };
                drawFill.ValueChanged +=
                    delegate(object sender, OnValueChangeEventArgs eventArgs)
                    {
                        DamageIndicator.Fill = eventArgs.GetNewValue<Circle>().Active;
                        DamageIndicator.FillColor = eventArgs.GetNewValue<Circle>().Color;
                    };

                //add to menu
                menu.AddSubMenu(drawMenu);
            }
        }

        private float GetComboDamage(Obj_AI_Base enemy)
        {
            if (enemy == null)
                return 0;

            double damage = 0d;

            if (Q.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.Q);

            if (W.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.W);

            if (R.IsReady())
                damage += Player.GetSpellDamage(enemy, SpellSlot.R);

            damage = ActiveItems.CalcDamage(enemy, damage);

            return (float)damage;
        }

        private void Combo()
        {
            UseSpells(menu.Item("UseQCombo", true).GetValue<bool>(), menu.Item("UseWCombo", true).GetValue<bool>(),
                menu.Item("UseECombo", true).GetValue<bool>(), menu.Item("UseRCombo", true).GetValue<bool>(), "Combo");
        }

        private void Harass()
        {
            UseSpells(menu.Item("UseQHarass", true).GetValue<bool>(), menu.Item("UseWHarass", true).GetValue<bool>(),
                menu.Item("UseEHarass", true).GetValue<bool>(), false, "Harass");
        }

        private void UseSpells(bool useQ, bool useW, bool useE, bool useR, string source)
        {
            if (source == "Harass" && !HasMana("Harass"))
                return;

            var range = Flash_Ready() ? R2.Range : Q.Range;
            var target = TargetSelector.GetTarget(range, TargetSelector.DamageType.Magical);
            
            if (!target.IsValidTarget(range))
                return;

            var dmg = GetComboDamage(target);

            if (useE && StunCount() == 3 && E.IsReady() && menu.Item("E_Stun", true).GetValue<bool>())
                E.Cast(packets());

            if (useR && R.IsReady())
            {   
                if (dmg > target.Health || !menu.Item("R_Killable", true).GetValue<KeyBind>().Active)
                {
                    if(OverKillCheck(target))
                    {
                        R.Cast(target, packets());
                        return;
                    }
                }

                R_MEC(true);
            }

            //items
            if (source == "Combo")
            {
                var itemTarget = TargetSelector.GetTarget(750, TargetSelector.DamageType.Physical);
                if (itemTarget != null)
                {
                    ActiveItems.Target = itemTarget;

                    //see if killable
                    if (dmg > itemTarget.Health - 50)
                        ActiveItems.KillableTarget = true;

                    ActiveItems.UseTargetted = true;
                }
            }

            if (useW && W.GetPrediction(target).Hitchance >= HitChance.High && W.IsReady())
            {
                W.Cast(target);
                return;
            }

            if (useQ && Q.IsReady())
            {
                Q.Cast(target, true);
            }

        }

        private bool OverKillCheck(Obj_AI_Hero target)
        {
            if (!menu.Item("R_OverKill", true).GetValue<bool>())
                return true;

            if (Q.IsKillable(target) || W.IsKillable(target))
                return false;

            return true;
        }

        private void R_MEC(bool mode)
        {
            var minRHit = menu.Item("useR_enemyCount_Combo", true).GetValue<Slider>().Value;

            if(!mode)
                minRHit = menu.Item("useR_enemyCount", true).GetValue<Slider>().Value;

            if (!R.IsReady() && minRHit == 6)
                return;

            foreach (var pred in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(R.Range)).
                Select(target => R.GetPrediction(target, true)).Where(pred => pred.Hitchance >= HitChance.High && pred.AoeTargetsHitCount >= minRHit))
            {
                R.Cast(pred.CastPosition);
            }
        }

        private void W_MEC()
        {
            var minWHit = menu.Item("useW_enemyCount", true).GetValue<Slider>().Value;

            if (!W.IsReady() && minWHit == 6)
                return;

            foreach (var pred in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(W.Range)).
                Select(target => W.GetPrediction(target, true)).Where(pred => pred.Hitchance >= HitChance.High && pred.AoeTargetsHitCount >= minWHit))
            {
                W.Cast(pred.CastPosition);
            }
        }

        private int StunCount()
        {
            var buff = Player.Buffs.FirstOrDefault(x => x.Name == "pyromania");
            if (buff != null)
                return buff.Count;
            return 0;
        }

        private bool HasStun()
        {
            return Player.HasBuff("pyromania_particle", true);
        }

        private void Charge()
        {
            if(HasStun())
                return;

            if (menu.Item("chargeInFountain", true).GetValue<bool>() && Player.InFountain())
            {
                if (W.IsReady())
                    W.Cast(Game.CursorPos);

                if (E.IsReady())
                    E.Cast();
            }

            if (Player.ManaPercentage() < menu.Item("chargeMana", true).GetValue<Slider>().Value || Player.IsRecalling() || !menu.Item("ChargeStun", true).GetValue<KeyBind>().Active)
                return;

            var useW = menu.Item("W_charge", true).GetValue<bool>();
            var useE = menu.Item("E_Charge", true).GetValue<bool>();

            if (useW && W.IsReady())
            {
                var minion = MinionManager.GetMinions(W.Range, MinionTypes.All, MinionTeam.NotAlly);

                if (minion.Count > 0)
                {
                    W.Cast(minion[0], packets());
                    return;
                }
                W.Cast(Game.CursorPos, packets());
                return;
            }

            if (useE && E.IsReady())
                E.Cast(packets());
        }

        private bool FlashStunCondition(Obj_AI_Hero target)
        {
            if (!target.IsValidTarget(1000))
                return false;

            if(!R.IsReady() && !(StunCount() == 3 && E.IsReady()))
                return false;

            var dmg = GetComboDamage(target);
            if (Player.Distance(target) > 600 && FullManaCast() && Flash_Ready() && dmg > target.Health)
                return true;

            return false;
        }

        private void FlashStunRawr()
        {
            if(!Flash_Ready())
                Combo();

            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);
            if (!FlashStunCondition(target))
                return;

            var pred = R2.GetPrediction(target, true);

            if (Player.Distance(target) > 600)
            {
                Player.Spellbook.CastSpell(_flashSlot, pred.CastPosition);
                if (StunCount() == 3)
                    E.Cast(packets());
                Utility.DelayAction.Add(50, () => R2.Cast(pred.CastPosition, packets()));
                Utility.DelayAction.Add(100, () => W.Cast(pred.CastPosition, packets()));
            }
        }

        private void SmartKs()
        {
            foreach (Obj_AI_Hero target in ObjectManager.Get<Obj_AI_Hero>().Where(x => x.IsValidTarget(Q.Range) && x.IsEnemy && !x.IsDead).OrderByDescending(GetComboDamage))
            {
                if (target != null)
                {
                    if (Q.IsKillable(target) && Q.IsReady())
                    {
                        Q.Cast(target, packets());
                        return;
                    }

                    if (Player.Distance(target.ServerPosition) <= W.Range &&
                        (Player.GetSpellDamage(target, SpellSlot.W)) > target.Health && W.IsReady())
                    {
                        W.Cast();
                        return;
                    }

                    if (menu.Item("R_To_KS", true).GetValue<bool>())
                    {
                        if (Player.Distance(target.ServerPosition) <= R.Range &&
                            (Player.GetSpellDamage(target, SpellSlot.R)) > target.Health && R.IsReady())
                        {
                            R.Cast(target);
                            return;
                        }
                    }
                }
            }
        }

        private void Farm()
        {
            List<Obj_AI_Base> allMinionsQ = MinionManager.GetMinions(Player.ServerPosition, Q.Range,MinionTypes.All, MinionTeam.NotAlly);
            List<Obj_AI_Base> allMinionsW = MinionManager.GetMinions(Player.ServerPosition, W.Range,MinionTypes.All, MinionTeam.NotAlly);

            var useQ = menu.Item("UseQFarm", true).GetValue<bool>();
            var useW = menu.Item("UseWFarm", true).GetValue<bool>();

            if (useQ && Q.IsReady())
            {
                Q.Cast(allMinionsQ[0]);
            }

            if (useW && W.IsReady())
            {
                var pred = W.GetLineFarmLocation(allMinionsW);

                if (pred.MinionsHit >= menu.Item("LaneClear_useW_minHit", true).GetValue<Slider>().Value)
                    W.Cast(pred.Position);
            }
        }

        private void LastHit()
        {
            if (!Q.IsReady())
                return;

            if (!menu.Item("Q_Stun_LastHit", true).GetValue<bool>() && HasStun())
                return;

            foreach (var minion in MinionManager.GetMinions(Player.Position, Q.Range).Where(minion => HealthPrediction.GetHealthPrediction(minion,
                       (int)(Q.Delay + (minion.Distance(Player) / Q.Speed))) < Player.GetSpellDamage(minion, SpellSlot.Q)
                       && HealthPrediction.GetHealthPrediction(minion, (int)(Q.Delay + (minion.Distance(Player) / Q.Speed))) > 0))
                Q.Cast(minion);
        }

        protected override void Game_OnGameUpdate(EventArgs args)
        {
            if (Player.IsDead)
                return;

            //ks check
            if (menu.Item("smartKS", true).GetValue<bool>())
                SmartKs();

            if (menu.Item("FlashKILL", true).GetValue<KeyBind>().Active)
            {
                Player.IssueOrder(GameObjectOrder.MoveTo, Game.CursorPos);
                FlashStunRawr();
            }
            else if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active)
            {
                Combo();
            }
            else
            {
                R_MEC(false);
                W_MEC();

                if (menu.Item("LastHitQ", true).GetValue<KeyBind>().Active || menu.Item("LastHitQToggle", true).GetValue<KeyBind>().Active)
                    LastHit();

                if (menu.Item("LaneClearActive", true).GetValue<KeyBind>().Active)
                    Farm();

                if (menu.Item("HarassActive", true).GetValue<KeyBind>().Active)
                    Harass();

                if (menu.Item("HarassActiveT", true).GetValue<KeyBind>().Active)
                    Harass();

                Charge();
            }

            int mode = menu.Item("R_Mode", true).GetValue<StringList>().SelectedIndex;
            var target = TargetSelector.GetTarget(2000, TargetSelector.DamageType.Magical);
            if (Tibbers == null || !Tibbers.IsValid || Tibbers.IsDead || Tibbers.Health < 0)
                return;

            if (mode == 0 && target.IsValidTarget(2000))
            {
                Player.IssueOrder(
                    Tibbers.Distance(target) > 210 ? GameObjectOrder.MovePet : GameObjectOrder.AutoAttackPet, target);
            }
            else if(mode == 1)
            {
                Player.IssueOrder(GameObjectOrder.MovePet, Player);
            }

        }


        private Obj_AI_Base Tibbers;
        protected override void GameObject_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals("Tibbers"))
                return;

            Game.PrintChat("SALICE SUMMONS TIBBERSSSS");
            Tibbers = (Obj_AI_Base)sender;
        }

        protected override void GameObject_OnDelete(GameObject sender, EventArgs args)
        {
            if (sender == null || !sender.IsValid || !sender.Name.Equals("Tibbers"))
                return;

            Tibbers = null;
        }

        protected override void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base unit, GameObjectProcessSpellCastEventArgs args)
        {
            if (unit.IsMe || unit.IsAlly || !(unit is Obj_AI_Hero))
                return;

            if (menu.Item("E_Block", true).GetValue<bool>())
            {
                if (xSLxOrbwalker.IsAutoAttack(args.SData.Name) && args.Target.IsMe && Player.Distance(args.End) < 450)
                {
                    E.Cast(packets());
                }
            }
        }

        protected override void BeforeAttack(Orbwalking.BeforeAttackEventArgs args)
        {
            if (menu.Item("ComboActive", true).GetValue<KeyBind>().Active && menu.Item("disableAA", true).GetValue<bool>())
                args.Process = !(Q.IsReady() || W.IsReady());
        }

        protected override void Drawing_OnDraw(EventArgs args)
        {
            if (menu.Item("Draw_Disabled", true).GetValue<bool>())
                return;

            if (menu.Item("Draw_Q", true).GetValue<bool>())
                if (Q.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, Q.Range, Q.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_W", true).GetValue<bool>())
                if (W.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, W.Range, W.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R.Range, R.IsReady() ? Color.Green : Color.Red);

            if (menu.Item("Draw_R2", true).GetValue<bool>())
                if (R.Level > 0)
                    Render.Circle.DrawCircle(Player.Position, R2.Range, R.IsReady() ? Color.Green : Color.Red);

            var target = TargetSelector.GetTarget(1000, TargetSelector.DamageType.Magical);

            if (target != null && R.IsReady() && menu.Item("FlashStunTarget", true).GetValue<bool>())
            {
                Vector2 wts = Drawing.WorldToScreen(target.Position);

                if(FlashStunCondition(target))
                    Drawing.DrawText(wts[0] - 20, wts[1], Color.White, "FLASH KILL NOWZ");
            }

        }

        protected override void Interrupter_OnPosibleToInterrupt(Obj_AI_Hero unit, Interrupter2.InterruptableTargetEventArgs spell)
        {
            if (spell.DangerLevel < Interrupter2.DangerLevel.Medium || unit.IsAlly)
                return;

            if (menu.Item("Interrupt", true).GetValue<bool>() && unit.IsValidTarget(Q.Range))
            {
                if (StunCount() == 3 && E.IsReady())
                    E.Cast();

                if (HasStun())
                {
                    if (Q.IsReady())
                        Q.Cast(unit);

                    if (W.IsReady())
                        W.Cast(unit);
                }
            }
        }

        protected override void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!menu.Item("W_Gap", true).GetValue<bool>())
                return;

            if (!HasStun() || !gapcloser.Sender.IsValidTarget(Q.Range))
                return;

            if (W.IsReady())
            {
                W.Cast(gapcloser.Sender);
                return;
            }
            if (Q.IsReady())
            {
                Q.Cast(gapcloser.Sender);
            }
        }

    }
}
