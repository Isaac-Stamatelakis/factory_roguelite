using System;
using System.Collections.Generic;
using System.Linq;
using Item.Slot;
using Items;
using Player;
using TMPro;

namespace Robot.Upgrades.Info.Instances
{
    internal class SelfRobotUpgradeInfo : RobotUpgradeInfo
    {
        public const ulong FLIGHT_COST = 1024;
        public const ulong SPEED_INCREASE_COST_PER_SECOND = 16;
        public const ulong JUMP_INCREASE_COST = 128;
        public const ulong BONUS_JUMP_COST = 512;
        public const ulong ROCKET_BOOTS_COST_PER_SECOND = 128;
        public const ulong TELEPORT_COST = 16384;
        public const int NANO_BOT_DELAY = 2;
        public const int HEALTH_PER_UPGRADE = 5;
        public const float NANO_BOT_TIME_PER_UPGRADE = 2f;
        
        public override List<TMP_Dropdown.OptionData> GetDropDownOptions()
        {
            return GlobalHelper.EnumToDropDown<RobotUpgrade>();
        }

        public override string GetDescription(int upgrade)
        {
            RobotUpgrade robotUpgrade = (RobotUpgrade)upgrade;
            switch (robotUpgrade)
            {
                case RobotUpgrade.Speed:
                    return "Increases robot move speed";
                case RobotUpgrade.JumpHeight:
                    return "Increases robot jump height";
                case RobotUpgrade.BonusJump:
                    return "Grants bonus jumps in the air to robot";
                case RobotUpgrade.RocketBoots:
                    return "Grants bonus jumps in the air to robot";
                case RobotUpgrade.Flight:
                    return "Grants flight";
                case RobotUpgrade.Reach:
                case RobotUpgrade.Dash:
                case RobotUpgrade.Hover:
                case RobotUpgrade.Teleport:
                case RobotUpgrade.Light:
                case RobotUpgrade.NightVision:
                case RobotUpgrade.Health:
                    return "Increases max health";
                case RobotUpgrade.Energy:
                    return "Increases max energy";
                case RobotUpgrade.NanoBots:
                    return $"After not taking damage for {NANO_BOT_DELAY} seconds, nanobots heal the robot";
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override string GetTitle(int upgrade)
        {
            return ((RobotUpgrade)upgrade).ToString();
        }

        public override IAmountFormatter GetAmountFormatter(int upgrade)
        {
            PlayerRobot playerRobot = PlayerManager.Instance.GetPlayer().PlayerRobot;
            RobotUpgrade robotUpgrade = (RobotUpgrade)upgrade;
            switch (robotUpgrade)
            {
                case RobotUpgrade.Speed:
                    return new RatioUpgradeFormatter("+", string.Empty,1/PlayerRobot.BASE_MOVE_SPEED,RatioUpgradeDisplayType.Percent);
                case RobotUpgrade.JumpHeight:
                    return new RatioUpgradeFormatter("+", string.Empty,1/playerRobot.JumpStats.jumpVelocity,RatioUpgradeDisplayType.Percent);
                case RobotUpgrade.BonusJump:
                    break;
                case RobotUpgrade.RocketBoots:
                    break;
                case RobotUpgrade.Flight:
                    break;
                case RobotUpgrade.Reach:
                    break;
                case RobotUpgrade.Dash:
                    break;
                case RobotUpgrade.Hover:
                    break;
                case RobotUpgrade.Teleport:
                    break;
                case RobotUpgrade.Light:
                    break;
                case RobotUpgrade.NightVision:
                    break;
                case RobotUpgrade.Health:
                    return new RatioUpgradeFormatter("+", string.Empty,HEALTH_PER_UPGRADE,RatioUpgradeDisplayType.Integer);
                case RobotUpgrade.Energy:
                    return new EnergyUpgradeFormatter();
                case RobotUpgrade.NanoBots:
                    return new RatioUpgradeFormatter(string.Empty,"S", NANO_BOT_TIME_PER_UPGRADE,RatioUpgradeDisplayType.Integer);
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public override IAmountFormatter GetEnergyCostFormatter(int upgrade)
        {
            RobotUpgrade robotUpgrade = (RobotUpgrade)upgrade;
            switch (robotUpgrade)
            {
                case RobotUpgrade.Speed:
                    return new EnergyCostFormatter(SPEED_INCREASE_COST_PER_SECOND);
                case RobotUpgrade.JumpHeight:
                    break;
                case RobotUpgrade.BonusJump:
                    return new EnergyCostFormatter(BONUS_JUMP_COST);
                case RobotUpgrade.RocketBoots:
                    return new EnergyCostFormatter(ROCKET_BOOTS_COST_PER_SECOND);
                case RobotUpgrade.Flight:
                    break;
                case RobotUpgrade.Reach:
                    break;
                case RobotUpgrade.Dash:
                    break;
                case RobotUpgrade.Hover:
                    break;
                case RobotUpgrade.Teleport:
                    break;
                case RobotUpgrade.Light:
                    break;
                case RobotUpgrade.NightVision:
                    break;
                case RobotUpgrade.Health:
                    break;
                case RobotUpgrade.Energy:
                    break;
                case RobotUpgrade.NanoBots:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public override List<int> GetContinuousUpgrades()
        {
            return new List<int>
            {
                (int)RobotUpgrade.Speed,
                (int)RobotUpgrade.JumpHeight,
                (int)RobotUpgrade.Reach
            };
        }

        public override List<int> GetConstantUpgrades()
        {
            return new List<int>
            {
                (int)RobotUpgrade.Health,
                (int)RobotUpgrade.Energy,
                (int)RobotUpgrade.NanoBots
            };
        }

        public override List<int> GetAllUpgrades()
        {
            return System.Enum.GetValues(typeof(RobotUpgrade)).Cast<int>().ToList();
        }

        private class EnergyUpgradeFormatter : IDiscreteUpgradeAmountFormatter
        {
            public string Format(int value)
            {
                int realValue = 2 << value;
                return ItemDisplayUtils.FormatAmountText((uint)realValue, false) + "J";
            }
        }
        
        private class EnergyCostFormatter : IContinousUpgradeAmountFormatter
        {
            private readonly ulong cost;

            public EnergyCostFormatter(ulong cost)
            {
                this.cost = cost;
            }

            public string Format(float upgrade)
            {
                return $"Requires: {cost * upgrade:F0}J/S";
            }
        }
        
        private class RatioUpgradeFormatter : IDiscreteUpgradeAmountFormatter, IContinousUpgradeAmountFormatter
        {
            private readonly float ratio;
            private readonly string prefix;
            private readonly string suffix;
            private readonly RatioUpgradeDisplayType displayType;

            public RatioUpgradeFormatter(string prefix, string suffix, float ratio, RatioUpgradeDisplayType ratioUpgradeDisplayType)
            {
                this.prefix = prefix;
                this.ratio = ratio;
                this.suffix = suffix;
                this.displayType = ratioUpgradeDisplayType;
            }

            public string Format(int value)
            {
                return Format((float)value);
            }

            public string Format(float upgrade)
            {
                float realValue = upgrade * ratio;
                switch (displayType)
                {
                    case RatioUpgradeDisplayType.Percent:
                        return $"{prefix}{realValue:P1}{suffix}";
                    case RatioUpgradeDisplayType.Integer:
                        return $"{prefix}{realValue:F0}{suffix}";
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
            }
        }

        private enum RatioUpgradeDisplayType
        {
            Percent,
            Integer
        }
    }

}