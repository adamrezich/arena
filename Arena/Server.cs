using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Arena {
	public class Server {
		public static Server Local;
		public readonly bool IsLocalServer;

		public Dictionary<int, Player> Players = new Dictionary<int, Player>();
		protected int playerIndex = 0;
		public Dictionary<int, Unit> Units = new Dictionary<int, Unit>();
		protected int unitIndex = 0;
		protected Dictionary<int, RemoteClient> RemoteClients = new Dictionary<int, RemoteClient>();

		public Server() {
			IsLocalServer = true;
		}
		
		public void AddPlayer(string name, int number, Teams team, Roles role) {
			Player player = new Player(name, number, team, role);
			RemoteClients.Add(++playerIndex, new RemoteClient());
			Players.Add(playerIndex, player);
			foreach(RemoteClient r in AllClients())
				r.SendNewPlayer(playerIndex);
			MakePlayerUnit(player, new Vector2(150, 150 * playerIndex));
		}
		public void AddBot(Teams team, Roles role) {
			Bot bot = new Bot(team, role);
			Players.Add(++playerIndex, bot);
			foreach(RemoteClient r in AllClients())
				r.SendNewPlayer(playerIndex);
			MakePlayerUnit(bot, new Vector2(150, 150 * playerIndex));
		}
		public void MakePlayerUnit(Player player, Vector2 position) {
			Unit u = new Unit(player, Role.List[player.Role].Health, Role.List[player.Role].Energy);
			u.Owner = player;
			u.Team = player.Team;
			u.JumpTo(position);
			Role.SetUpUnit(ref u, player.Role);
			player.ControlledUnits.Add(u);
			player.PlayerUnit = u;

			Units.Add(++unitIndex, u);
			foreach (RemoteClient r in AllClients())
				r.SendNewPlayerUnit(unitIndex, GetPlayerID(player));
		}
		public void RecieveAttackOrder(int attackerIndex, int victimIndex) {
			Console.WriteLine("Recieving attack order: " + Units[attackerIndex].Owner.Name + " -> " + Units[victimIndex].Owner.Name);
			Units[attackerIndex].AttackTarget = Units[victimIndex];
			foreach (RemoteClient r in AllClientsButOne(GetPlayerID(Units[attackerIndex].Owner)))
				r.SendAttackOrder(Units[attackerIndex], Units[victimIndex]);
		}
		public void RecieveMoveOrder(int unitIndex, float x, float y) {
			Unit u = Units[unitIndex];
			u.AttackTarget = null;
			u.IntendedPosition = new Vector2(x, y);
			foreach (RemoteClient r in AllClientsButOne(GetPlayerID(Units[unitIndex].Owner)))
				r.SendMoveOrder(Units[unitIndex], u.IntendedPosition);
		}
		public int GetPlayerID(UnitController player) {
			return Players.FirstOrDefault(x => x.Value == player).Key;
		}
		public int GetUnitID(Unit unit) {
			return Units.FirstOrDefault(x => x.Value == unit).Key;
		}

		protected List<RemoteClient> AllClientsButOne(int one) {
			Dictionary<int, RemoteClient> dict = new Dictionary<int, RemoteClient>(RemoteClients);
			if (dict.ContainsKey(one)) dict.Remove(one);
			return dict.Values.ToList<RemoteClient>();
		}
		protected List<RemoteClient> AllClients() {
			return RemoteClients.Values.ToList<RemoteClient>();
		}

		public void Update(GameTime gameTime) {
			foreach (KeyValuePair<int, Player> kvp in Players)
				kvp.Value.Update(gameTime);
			foreach (KeyValuePair<int, Unit> kvp in Units)
				kvp.Value.Update(gameTime);
		}

		protected class RemoteClient {
			public void SendAttackOrder(Unit attacker, Unit victim) {
				if (Server.Local.IsLocalServer) {
					Client.Local.RecieveAttackOrder(Server.Local.GetUnitID(attacker), Server.Local.GetUnitID(victim));
				}
			}
			public void SendMoveOrder(Unit unit, Vector2 position) {
				if (Server.Local.IsLocalServer) {
					Client.Local.RecieveMoveOrder(Server.Local.GetUnitID(unit), position.X, position.Y);
				}
			}
			public void SendNewPlayerUnit(int unitIndex, int playerIndex) {
				if (Server.Local.IsLocalServer) {
					Unit u = Server.Local.Units[unitIndex];
					Client.Local.RecieveNewPlayerUnit(unitIndex, playerIndex, u.Position.X, u.Position.Y, u.Direction);
				}
			}
			public void SendNewPlayer(int index) {
				if (Server.Local.IsLocalServer) {
					Player p = Server.Local.Players[index];
					Client.Local.RecieveNewPlayer(index, p.Name, p.Number, p.Team, p.Role);
				}
			}
		}
	}
}

