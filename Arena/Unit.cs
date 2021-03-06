using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VGame;

namespace Arena {
	public enum Teams {
		None,
		Home,
		Away,
		Neutral
	}
	public enum Attitude {
		Neutral,
		Friend,
		Enemy
	}
	public class Unit : Entity {
		public UnitController Owner;
		public Actor Actor;
		public Teams Team;

		public double MoveSpeed {
			get {
				return baseMoveSpeed + BuffTotal(BuffType.MoveSpeed);
			}
			set {
				baseMoveSpeed = value;
			}
		}
		public double TurnSpeed {
			get {
				return baseTurnSpeed + BuffTotal(BuffType.TurnSpeed);
			}
			set {
				baseTurnSpeed = value;
			}
		}
		public int MaxHealth {
			get {
				return baseMaxHealth + (int)BuffTotal(BuffType.MaxHealth);
			}
			set {
				baseMaxHealth = value;
			}
		}
		public int MaxEnergy {
			get {
				return baseMaxEnergy + (int)BuffTotal(BuffType.MaxEnergy);
			}
			set {
				baseMaxEnergy = value;
			}
		}
		public double HealthRegen {
			get {
				return baseHealthRegen + BuffTotal(BuffType.HealthRegen);
			}
			set {
				baseHealthRegen = value;
			}
		}
		public double EnergyRegen {
			get {
				return baseEnergyRegen + BuffTotal(BuffType.EnergyRegen);
			}
			set {
				baseEnergyRegen = value;
			}
		}
		public double BaseAttackTime {
			get {
				return baseBaseAttackTime + BuffTotal(BuffType.BaseAttackTime);
			}
			set {
				baseBaseAttackTime = value;
			}
		}
		public double AttackSpeed {
			get {
				return baseAttackSpeed + BuffTotal(BuffType.AttackSpeed);
			}
			set {
				baseAttackSpeed = value;
			}
		}
		public int AttackRange {
			get {
				return baseAttackRange + (int)BuffTotal(BuffType.AttackRange);
			}
			set {
				baseAttackRange = value;
			}
		}
		public int AttackDamage {
			get {
				return baseAttackDamage + (int)BuffTotal(BuffType.AttackDamage);
			}
			set {
				baseAttackDamage = value;
			}
		}
		public int Health { get; set; }
		public int Energy { get; set; }
		public int Level { get; set; }
		public int Experience { get; set; }
		public Vector2 Position { get; set; }
		public Vector2 IntendedPosition { get; set; }
		public double Direction { get; set; }
		public double IntendedDirection { get; set; }
		public Unit AttackTarget { get; set; }

		public bool AutoAttacking = false;
		public List<Buff> Buffs {
			get {
				if (newBuffs.Count == 0)
					return buffs;
				List<Buff> total = new List<Buff>(buffs);
				total.AddRange(newBuffs);
				return total;
			}
		}
		protected List<Buff> buffs = new List<Buff>();
		protected List<Buff> newBuffs = new List<Buff>();
		public TimeSpan NextAutoAttackReady = new TimeSpan();
		public List<Ability> Abilities = new List<Ability>();
		public Vector2 LastPosition;
		private double healthRegenPart = 0;
		private double energyRegenPart = 0;

		public double GetHealthPercent() {
			return (double)Health / (double)MaxHealth;
		}
		public double GetEnergyPercent() {
			return (double)Energy / (double)MaxEnergy;
		}
		public double GetExperiencePercent() {
			return (double)(Experience % 100) / 100;
		}
		public int ExperienceLeftOver() {
			return Experience % 100;
		}

		private double baseMoveSpeed;
		private double baseTurnSpeed;
		private int baseMaxHealth;
		private int baseMaxEnergy;
		private double baseHealthRegen;
		private double baseEnergyRegen;
		private double baseBaseAttackTime;
		private double baseAttackSpeed = 1;
		private int baseAttackRange;
		private int baseAttackDamage;

		public Unit(UnitController owner, int maxHealth, int maxEnergy) {
			Owner = owner;
			MaxHealth = maxHealth;
			MaxEnergy = maxEnergy;
			Health = MaxHealth;
			Energy = MaxEnergy;
			Direction = 0;
			IntendedDirection = 0;
			AttackTarget = null;
			Level = 0;
			Experience = 0;
		}

		public void Update(GameTime gameTime) {
			for (var i = 0; i < newBuffs.Count; i++) {
				newBuffs[i].ExpirationTime += gameTime.TotalGameTime;
				buffs.Add(newBuffs[i]);
			}
			newBuffs.Clear();
			for (var i = 0; i < buffs.Count; i++)
				if (!buffs[i].Permanent)
					if (gameTime.TotalGameTime >= buffs[i].ExpirationTime)
						buffs.RemoveAt(i);

			Regen();
			foreach (Ability a in Abilities)
				a.Update(gameTime);
			MoveTowardsIntended();
			if (Direction != IntendedDirection)
				Direction = Direction.LerpAngle(IntendedDirection, (double)TurnSpeed / 10 / MathHelper.PiOver2);
			LastPosition = Position;
			if (AutoAttacking) {
				AutoAttack(gameTime);
			}
		}
		public void AutoAttack(GameTime gameTime) {
			if (gameTime.TotalGameTime > NextAutoAttackReady && !(Owner is Bot)) {
				NextAutoAttackReady = gameTime.TotalGameTime + TimeSpan.FromSeconds(BaseAttackTime / AttackSpeed);
				AttackTarget.Health = Math.Max(AttackTarget.Health - AttackDamage, 0);
				if (Client.Local != null)
					Client.Local.MakeEffect(new Effects.AutoAttackBeam(gameTime, Position, AttackTarget.Position));
			}
		}
		public void Regen() {
			if (Health < MaxHealth)
				healthRegenPart += HealthRegen;
			else
				healthRegenPart = 0;
			if (healthRegenPart >= 1) {
				healthRegenPart = 0;
				Health = Math.Min(Health + 1, MaxHealth);
			}
			if (Energy < MaxEnergy)
				energyRegenPart += EnergyRegen;
			else
				energyRegenPart = 0;
			if (energyRegenPart >= 1) {
				energyRegenPart = 0;
				Energy = Math.Min(Energy + 1, MaxEnergy);
			}
		}
		public void JumpTo(Vector2 position) {
			Position = position;
			IntendedPosition = position;
		}
		public void MoveTowardsIntended() {
			AutoAttacking = false;
			Vector2 intendedPosition;
			if (AttackTarget != null) {
				intendedPosition = AttackTarget.Position;
				TurnTowards(intendedPosition);
				if (Vector2.Distance(Position, intendedPosition) <= AttackRange) {
					if (Math.Abs(MathHelper.WrapAngle((float)(Direction - IntendedDirection))) < MathHelper.Pi / 8) {
						// AUTOATTACK
						AutoAttacking = true;
						return;
					}
					return;
				}
			}
			else
				intendedPosition = IntendedPosition;
			if (Vector2.Distance(Position, intendedPosition) > MoveSpeed) {
				Vector2 velocity = Vector2.Normalize(intendedPosition - Position);
				if (Math.Abs(MathHelper.WrapAngle((float)(Direction - IntendedDirection))) < MathHelper.Pi / 8) {
					//Actor foundActor = null;
					//if (foundActor == null)
						MoveInDirection(velocity, MoveSpeed);
					//else {
					//}
				}
				TurnTowards(intendedPosition);
			}
			else
				Position = IntendedPosition;
		}
		public void MoveInDirection(Vector2 direction, double speed) {
			Position += direction * (float)speed;
		}
		public void TurnTowards(Vector2 position) {
			IntendedDirection = Math.Atan2(position.Y - Position.Y, position.X - Position.X);
		}
		public bool CanUseAbility(int ability) {
			return (Abilities.Count >= ability + 1 && Abilities[ability].Level > 0 && Energy >= Abilities[ability].EnergyCost && Abilities[ability].Ready && Abilities[ability].ActivationType != AbilityActivationType.Passive);
		}
		public void UseAbility(int ability, float? val1, float? val2) {
			Abilities[ability].Activate(val1, val2);
		}
		public bool CanLevelUp(int ability) {
			return (Abilities[ability].Level < Abilities[ability].Levels);
		}
		public void LevelUp(int ability) {
			if (Abilities[ability].Level == 0)
				Abilities[ability].ReadyTime = TimeSpan.Zero;
			Abilities[ability].Level += 1;
			if (Abilities[ability].ActivationType == AbilityActivationType.Passive)
				UseAbility(ability, null, null);
		}
		public void AddBuff(Buff buff) {
			newBuffs.Add(buff);
		}
		public Attitude AttitudeTowards(UnitController unitController) {
			if (unitController.Team != Team)
				return Attitude.Enemy;
			return Attitude.Friend;
		}
		public double BuffTotal(BuffType type) {
			double returnValue = 0;
			foreach (Buff b in Buffs)
				returnValue += b.ValueOf(type);
			return returnValue;
		}
	}
	public abstract class UnitController {
		public Unit CurrentUnit {
			get {
				if (ControlledUnits.Count > 0)
					return ControlledUnits[currentUnit];
				else
					return null;
			}
			set {
				if (!ControlledUnits.Contains(value))
					ControlledUnits.Add(value);
				currentUnit = ControlledUnits.IndexOf(value);
			}
		}
		protected int currentUnit;
		public List<Unit> ControlledUnits = new List<Unit>();
		public abstract string Name { get; }
		public Teams Team;
		public abstract void Update(GameTime gameTime);
	}
	public class CreepController : UnitController {
		public CreepController(Teams team) {
			Team = team;
		}
		public void SpawnCreep(Vector2 position) {

		}
		public override string Name {
			get {
				return "CREEP CONTROLLER";
			}
		}
		public override void Update(GameTime gameTime) {
		}
	}
}

